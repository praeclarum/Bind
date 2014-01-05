//
//  Copyright 2013-2014 Frank A. Krueger
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;

namespace Praeclarum.Bind
{
	using Arg = KeyValuePair<ParameterExpression, object>;

	public class Subscription
	{
		readonly Action<int> action;

		public object Target { get; private set; }
		public MemberInfo Member { get; private set; }

		public Subscription (object target, MemberInfo member, Action<int> action)
		{
			this.Target = target;
			if (member == null)
				throw new ArgumentNullException ("member");
			this.Member = member;
			if (action == null)
				throw new ArgumentNullException ("action");
			this.action = action;
		}

		public void Notify (int changeId)
		{
			action (changeId);
		}
	}

	public abstract class Binding
	{
		public object Value { get; protected set; }

		protected Binding (object value)
		{
			Value = value;
		}
		public virtual void Unbind ()
		{
		}

		public static Binding Create<T> (Expression<Func<T>> expr, params Arg[] args)
		{
			return BindAny (expr, args);
		}

		public static Binding Create (LambdaExpression expr, params Arg[] args)
		{
			return BindAny (expr, args);
		}

		static Binding BindAny (LambdaExpression expr, params Arg[] args)
		{
			var body = expr.Body;

			//
			// Are we binding two values?
			//
			if (body.NodeType == ExpressionType.Equal) {
				var b = (BinaryExpression)body;
				return new EqualityBinding (b.Left, b.Right, args);
			}

			//
			// This must be a new object binding (a template)
			//
			return new NewObjectBinding (expr, args);
		}

		protected static bool SetValue (Expression expr, object value, int changeId, params Arg[] args)
		{
			if (expr.NodeType == ExpressionType.MemberAccess) {				
				var m = (MemberExpression)expr;
				var mem = m.Member;

				var target = Eval (m.Expression, args);

				if (mem.MemberType == MemberTypes.Field) {
					var f = (FieldInfo)mem;
					f.SetValue (target, value);
				} else if (mem.MemberType == MemberTypes.Property) {
					var p = (PropertyInfo)mem;
					p.SetValue (target, value, null);
				} else {
					ReportError ("Trying to SetValue on " + mem.MemberType + " member");
					return false;
				}

				Invalidate (target, mem, changeId);
				return true;
			}

			ReportError ("Trying to SetValue on " + expr.NodeType + " expression");
			return false;
		}

		public static event Action<string> Error = delegate {};

		static void ReportError (string message)
		{
			Debug.WriteLine (message);
			Error (message);
		}

		static void ReportError (Exception ex)
		{
			ReportError (ex.ToString ());
		}

		#region Change Notification

		class ObjectSubscriptions
		{
			readonly object target;
			readonly MemberInfo member;

			EventInfo eventInfo;
			Delegate eventHandler;
			
			public ObjectSubscriptions (object target, MemberInfo mem)
			{
				this.target = target;
				this.member = mem;
			}

			void SubscribeEvent ()
			{
				if (target != null) {
					var npc = target as INotifyPropertyChanged;
					if (npc != null && member.MemberType == MemberTypes.Property) {
						// TODO: INotifyPropertyChanged
					}
					else {
						// Look for Changed event
						var ty = target.GetType ();
						SubscribeEvent (target, ty, member.Name + "Changed", "EditingDidEnd", "ValueChanged", "Changed");
					}
				}
			}

			void SubscribeEvent (object target, Type type, params string[] names)
			{
				foreach (var name in names) {
					var ev = type.GetEvent (name);

					if (ev != null) {
						if (typeof(EventHandler).IsAssignableFrom (ev.EventHandlerType)) {
							eventInfo = ev;
							eventHandler = (EventHandler)HandleEventHandler;
							ev.AddEventHandler (target, eventHandler);
							return;
						} else {
							ReportError ("Cannot use event " + name + " because it is not an EventHandler");
						}
					}
				}
			}

			void UnsubscribeEvent ()
			{
				if (eventInfo == null)
					return;

				eventInfo.RemoveEventHandler (target, eventHandler);

				eventInfo = null;
				eventHandler = null;
			}

			void HandleEventHandler (object sender, EventArgs e)
			{
				Binding.Invalidate (target, member);
			}

			readonly List<Subscription> subscriptions = new List<Subscription> ();

			public void Add (Subscription sub)
			{
				if (subscriptions.Count == 0) {
					SubscribeEvent ();
				}

				subscriptions.Add (sub);
			}

			public void Remove (Subscription sub)
			{
				subscriptions.Remove (sub);

				if (subscriptions.Count == 0) {
					UnsubscribeEvent ();
				}
			}

			public void Notify (int changeId)
			{
				foreach (var s in subscriptions) {
					s.Notify (changeId);
				}
			}
		}

		static Dictionary<Tuple<Object, MemberInfo>, ObjectSubscriptions> objectSubs = new Dictionary<Tuple<Object, MemberInfo>, ObjectSubscriptions> ();

		public static Subscription Subscribe (object target, MemberInfo member, Action<int> k)
		{
			var key = Tuple.Create (target, member);
			ObjectSubscriptions subs;
			if (!objectSubs.TryGetValue (key, out subs)) {
				subs = new ObjectSubscriptions (target, member);
				objectSubs.Add (key, subs);
			}

			Debug.WriteLine ("SUBSCRIBE " + target + " " + member);
			var sub = new Subscription (target, member, k);
			subs.Add (sub);
			return sub;
		}

		public static void Unsubscribe (Subscription sub)
		{
			var key = Tuple.Create (sub.Target, sub.Member);
			ObjectSubscriptions subs;
			if (objectSubs.TryGetValue (key, out subs)) {
				Debug.WriteLine ("UNSUBSCRIBE " + sub.Target + " " + sub.Member);
				subs.Remove (sub);
			}
		}

		public static void Invalidate<T> (Expression<Func<T>> lambdaExpr, int changeId = 0)
		{
			var body = lambdaExpr.Body;
			if (body.NodeType == ExpressionType.MemberAccess) {
				var m = (MemberExpression)body;
				var obj = Eval (m.Expression);
				Invalidate (obj, m.Member, changeId);
			}
		}

		public static void Invalidate (object obj, MemberInfo member, int changeId = 0)
		{
			var key = Tuple.Create (obj, member);
			ObjectSubscriptions subs;
			if (objectSubs.TryGetValue (key, out subs)) {
				Debug.WriteLine ("INVALIDATE {0} {1}", obj, member.Name);
				subs.Notify (changeId);
			}
		}

		#endregion
		
		public static object Eval (Expression expr, params Arg[] args)
		{
			//
			// Easy case
			//
			if (expr.NodeType == ExpressionType.Constant) {
				return ((ConstantExpression)expr).Value;
			}
			
			//
			// General case
			//
//			Console.WriteLine ("WARNING EVAL COMPILED {0}", expr);
			var lambda = Expression.Lambda (expr, args.Select (a => a.Key).ToArray ());
			return lambda.Compile ().DynamicInvoke (args.Select (a => a.Value).ToArray ());
		}
	}

	/// <summary>
	/// Binding between two values.
	/// </summary>
	public class EqualityBinding : Binding
	{
		class Trigger
		{
			public Expression Expression;
			public MemberInfo Member;
			public Subscription Subscription;
		}
		
		//		Expression left, right;
		Arg[] args;
		
		List<Trigger> leftTriggers = new List<Trigger> ();
		List<Trigger> rightTriggers = new List<Trigger> ();
		
		public EqualityBinding (Expression left, Expression right, params Arg[] args)
			: base (null)
		{
			this.args = args;

			// Try evaling the right and assigning left
			Value = Eval (right, args);
			var leftSet = SetValue (left, Value, nextChangeId, args);

			// If that didn't work, then try the other direction
			if (!leftSet) {
				Value = Eval (left, args);
				SetValue (right, Value, nextChangeId, args);
			}

			nextChangeId++;

			CollectTriggers (left, leftTriggers);
			CollectTriggers (right, rightTriggers);

			Resubscribe (leftTriggers, left, right);
			Resubscribe (rightTriggers, right, left);
		}

		public override void Unbind ()
		{
			Unsubscribe (leftTriggers);
			Unsubscribe (rightTriggers);
			base.Unbind ();
		}

		void Resubscribe (List<Trigger> triggers, Expression expr, Expression dependentExpr)
		{
			Unsubscribe (triggers);
			Subscribe (triggers, changeId => OnSideChanged (triggers, expr, dependentExpr, changeId));
		}

		int nextChangeId = 1;
		readonly HashSet<int> activeChangeIds = new HashSet<int> ();
		
		void OnSideChanged (List<Trigger> triggers, Expression expr, Expression dependentExpr, int causeChangeId)
		{
			if (activeChangeIds.Contains (causeChangeId))
				return;

			var v = Eval (expr, args);
			
			if (v == null && Value == null)
				return;
			
			if ((v == null && Value != null) ||
				(v != null && Value == null) ||
				((v is IComparable) && ((IComparable)v).CompareTo (Value) != 0)) {
				
				Value = v;

				var changeId = nextChangeId++;
				activeChangeIds.Add (changeId);
				SetValue (dependentExpr, v, changeId, args);
				activeChangeIds.Remove (changeId);
			} else {
				Debug.WriteLine ("Prevented needless update");
			}
		}

		void Unsubscribe (List<Trigger> triggers)
		{
			foreach (var t in triggers) {
				if (t.Subscription != null) {
					Unsubscribe (t.Subscription);
				}
			}
		}
		
		void Subscribe (List<Trigger> triggers, Action<int> k)
		{
			foreach (var t in triggers) {
				t.Subscription = Subscribe (Eval (t.Expression), t.Member, k);
			}
		}		
		
		void CollectTriggers (Expression s, List<Trigger> triggers)
		{
			if (s.NodeType == ExpressionType.MemberAccess) {
				
				var m = (MemberExpression)s;
				CollectTriggers (m.Expression, triggers);
				var t = new Trigger { Expression = m.Expression, Member = m.Member };
				triggers.Add (t);

			} else if (s is BinaryExpression) {
				var b = (BinaryExpression)s;
				
				CollectTriggers (b.Left, triggers);
				CollectTriggers (b.Right, triggers);
			}
		}
	}
	
	public class NewObjectBinding : Binding
	{
		public NewObjectBinding (Expression expression, params Arg[] args)
			: base (null)
		{
			BindNewObject (expression, args);
		}

		static object BindNewObject (Expression expression, params Arg[] args)
		{
			switch (expression.NodeType) {
			case ExpressionType.MemberInit:
				return BindMemberInit ((MemberInitExpression)expression, args);
			case ExpressionType.Call:
				return BindMethodCall ((MethodCallExpression)expression, args);
			case ExpressionType.NewArrayInit:
				return BindNewArrayInit ((NewArrayExpression)expression, args);
			case ExpressionType.MemberAccess:
				return BindMember ((MemberExpression)expression, args);
			case ExpressionType.Add:
				return BindAdd ((BinaryExpression)expression, args);
			case ExpressionType.Constant:
				return BindConstant ((ConstantExpression)expression, args);
			default:
				throw new NotSupportedException (expression.NodeType + "");
			}
		}

		static object BindConstant (ConstantExpression constExpression, params Arg[] args)
		{
			var v = constExpression.Value;
			return v;
		}

		static object BindAdd (BinaryExpression memberExpression, params Arg[] args)
		{
			var v = Eval (memberExpression, args);
			return v;
		}
		
		static object BindMember (MemberExpression memberExpression, params Arg[] args)
		{
			var v = Eval (memberExpression, args);
			return v;
		}
		
		static object BindMemberInit (MemberInitExpression memberInit, params Arg[] args)
		{
			var objType = memberInit.Type;
			var obj = Activator.CreateInstance (objType);
			
			foreach (var binding in memberInit.Bindings) {
				if (binding.BindingType == MemberBindingType.Assignment) {
					var assignment = (MemberAssignment)binding;
					var member = binding.Member;
					
					var boundValue = BindNewObject (assignment.Expression, args);
					
					if (member.MemberType == System.Reflection.MemberTypes.Property) {
						var prop = (PropertyInfo)member;
						prop.SetValue (obj, boundValue, null);
					}
					else {
						throw new NotSupportedException (member.MemberType + "");
					}
				}
				else {
					throw new NotSupportedException (binding.BindingType + "");
				}
			}
			
			return obj;
		}
		
		static object BindMethodCall (MethodCallExpression methodCall, Arg[] args)
		{
			var method = methodCall.Method;
			
			if (typeof(IEnumerable).IsAssignableFrom (method.ReturnType)) {
				
				if (methodCall.Method.Name == "Select") {
					return new SelectLinqCollection (methodCall, args);
				}
				else {
					throw new NotSupportedException (methodCall + "");
				}
				
			}
			else {
				throw new NotSupportedException (methodCall + "");
			}
		}

		static object BindNewArrayInit (NewArrayExpression newArray, Arg[] args)
		{
			var a = Array.CreateInstance (newArray.Type.GetElementType (), newArray.Expressions.Count);// Activator.CreateInstance (newArray.Type);

			var i = 0;
			foreach (var e in newArray.Expressions) {
				var v = BindNewObject (e, args);
				a.SetValue (v, i);
				i++;
			}

			return a;
		}
	}
	
	class BoundLinqCollection : ObservableCollection<object>
	{
		protected readonly IEnumerable source;
		
		public BoundLinqCollection (MethodCallExpression methodCall, Arg[] args)
		{
			source = (IEnumerable)Binding.Eval (methodCall.Arguments [0], args);
			
			var cc = source as INotifyCollectionChanged;
			if (cc != null) {
				cc.CollectionChanged += HandleSourceChanged;;
			}
		}
		
		protected virtual void HandleSourceChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
		}
	}
	
	class SelectLinqCollection : BoundLinqCollection
	{
		LambdaExpression createExpression;
		Arg[] args;
		
		public SelectLinqCollection (MethodCallExpression methodCall, Arg[] args)
			: base (methodCall, args)
		{
			createExpression = (LambdaExpression)methodCall.Arguments [1];
			this.args = args;
			
			foreach (var sourceItem in source) {
				Add (Project (sourceItem));
			}
		}
		
		protected override void HandleSourceChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null) {
				var i = e.NewStartingIndex;
				foreach (var sourceItem in e.NewItems) {
					Insert (i, Project (sourceItem));
					i++;
				}
			}
		}
		
		object Project (object x)
		{
			var binding = Binding.Create (
				createExpression,
				args.Concat (new[] { new Arg (createExpression.Parameters[0], x) }).ToArray ());
			return binding.Value;
		}
	}
}










