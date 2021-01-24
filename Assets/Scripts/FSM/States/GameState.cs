using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class GameState : IState
{
    private GameObject ui;

    private CompositeDisposable disposables;

    public GameState()
    {
        disposables = new CompositeDisposable();
    }
    public void BeginEnter()
    {
        InitUI();
    }

    public void EndEnter()
    {
    }

    public IEnumerable Execute()
    {
        yield return null;
    }

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;
    public void EndExit()
    {
        disposables.Clear();
        GameObject.Destroy(ui);
    }
    
    private void InitUI()
    {
        ui = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Game UI"));
    }
}
