////////////////////////////////////////////////////////////////////////////////////////////////////
// About:   Finite State Machine by Jackson Dunstan
// Article: http://JacksonDunstan.com/articles/3137
// License: MIT
// Copyright © 2015 Jackson Dunstan
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////
 
using System;
using System.Collections;
using System.Collections.Generic;
 
/// <summary>
/// Event args that are dispatched when a state wants to transition to another state
/// </summary>
public class StateBeginExitEventArgs : EventArgs
{
	/// <summary>
	/// The state to transition to
	/// </summary>
	public IState NextState { get; private set; }
 
	/// <summary>
	/// The transition to use to get to the next state
	/// </summary>
	public IStateTransition Transition { get; private set; }
 
	/// <summary>
	/// Create the event args
	/// </summary>
	/// <param name="nextState">The state to transition to</param>
	/// <param name="transition">The transition to use to get to the next state</param>
	public StateBeginExitEventArgs(
		IState nextState,
		IStateTransition transition
	)
	{
		NextState = nextState;
		Transition = transition;
	}
}
 
/// <summary>
/// A mode of the app that executes over time. This begins the transition process and receives
/// notifications about its progress.
/// </summary>
public interface IState
{
	/// <summary>
	/// Notify the state that the transition process has begun the entering phase
	/// </summary>
	void BeginEnter();
 
	/// <summary>
	/// Notify the state that the transition process has finished the entering phase
	/// </summary>
	void EndEnter();
 
	/// <summary>
	/// Execute the state's logic over time. This function should 'yield return' until it has
	/// nothing left to do. It may also dispatch OnBeginExit when it is ready to begin transitioning
	/// to the next state. If it does this, this funtion will no longer be resumed.
	/// </summary>
	IEnumerable Execute();
 
	/// <summary>
	/// Dispatched when the state is ready to begin transitioning to the next state. This implies
	/// that the transition process will immediately begin the exiting phase.
	/// </summary>
	event EventHandler<StateBeginExitEventArgs> OnBeginExit;
 
	/// <summary>
	/// Notify the state that the transition process has finished exiting phase.
	/// </summary>
	void EndExit();
}
 
/// <summary>
/// A transition from one state to another. This is broken into two parts. The first is the 'exit'
/// process where the current state is left. The second is the 'enter' process where the next state
/// becomes the new current state. Both of these processes are executed over time.
/// </summary>
public interface IStateTransition
{
	/// <summary>
	/// Exit the current state over time. This function should 'yield return' until the exit process
	/// is finished.
	/// </summary>
	IEnumerable Exit();
 
	/// <summary>
	/// Enter the next state over time. This function should 'yield return' until the enter process
	/// is finished.
	/// </summary>
	IEnumerable Enter();
}
 
/// <summary>
/// A finite state machine that runs a single state at a time and handles the transition process
/// from one state to another
/// </summary>
public interface IStateMachine
{
	/// <summary>
	/// Execute the state machine by executing states and transitions. This function should
	/// 'yield return' until there are no more states to execute.
	/// </summary>
	IEnumerable Execute();
}
 
/// <summary>
/// A state machine implementation that executes states and transitions
/// </summary>
public class StateMachine : IStateMachine
{
	/// <summary>
	/// The current state
	/// </summary>
	private IState state;
 
	/// <summary>
	/// The next state (to transition to)
	/// </summary>
	private IState nextState;
 
	/// <summary>
	/// Transition to use to get to the next state
	/// </summary>
	private IStateTransition transition;
 
	/// <summary>
	/// Create the state machine with an initial state
	/// </summary>
	/// <param name="initialState">Initial state. BeginEnter() and EndEnter() will be called
	/// on it immediately</param>
	public StateMachine(IState initialState)
	{
		State = initialState;
		state.EndEnter();
	}
 
	/// <summary>
	/// Execute the initial state and any subsequent states and transitions until there are no more
	/// states to execute.
	/// </summary>
	public IEnumerable Execute()
	{
		while (true)
		{
			// Execute the current state until it transitions or stops executing
			for (
				var e = state.Execute().GetEnumerator();
				transition == null && e.MoveNext();
			)
			{
				yield return e.Current;
			}
 
			// Wait until the current state transitions
			while (transition == null)
			{
				yield return null;
			}
 
			// Stop listening for the current state to transition
			// This prevents accidentally transitioning twice
			state.OnBeginExit -= HandleStateBeginExit;
 
			// There is no next state to transition to
			// This means the state machine is finished executing
			if (nextState == null)
			{
				break;
			}
 
			// Run the transition's exit process
			foreach (var e in transition.Exit())
			{
				yield return e;
			}
			state.EndExit();
 
			// Switch state
			State = nextState;
			nextState = null;
 
			// Run the transition's enter process
			foreach (var e in transition.Enter())
			{
				yield return e;
			}
			transition = null;
			state.EndEnter();
		}
	}
 
	/// <summary>
	/// Set the current state, call its BeginEnter(), and listen for it to transition
	/// </summary>
	/// <value>The state to be the new current state</value>
	private IState State
	{
		set
		{
			state = value;
			state.OnBeginExit += HandleStateBeginExit;
			state.BeginEnter();
		}
	}
 
	/// <summary>
	/// Handles the current state wanting to transition
	/// </summary>
	/// <param name="sender">The state that wants to transition</param>
	/// <param name="e">Information about the desired transition</param>
	private void HandleStateBeginExit(object sender, StateBeginExitEventArgs e)
	{
		nextState = e.NextState;
		transition = e.Transition;
	}
}