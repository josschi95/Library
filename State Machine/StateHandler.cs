  private void Update()
  {
      HandleStateMachine();
  }

    private void HandleStateMachine()
    {
        if (currentPhase != null)
        {
            State nextPhase = currentPhase.Tick(character);
            if (nextPhase != null) SwitchToNextState(nextPhase);
        }
        else currentPhase = stateMachine.beginTurn;
    }

    private void SwitchToNextState(State phase)
    {
        if (currentPhase != phase)
        {
            currentPhase.OnStateExit(character);

            currentPhase = phase.OnStateEnter(character);
        }
        else currentPhase = phase;
    }
