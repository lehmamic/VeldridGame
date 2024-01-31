using Veldrid;

namespace VeldridGame.Input;

public class InputSystem
{
    public InputSystem()
    {
        var keys = Enum.GetValues<Key>().Distinct().ToArray();
        var keyboardState = new KeyboardState(
            keys.ToDictionary(key => key, key => false),
            keys.ToDictionary(key => key, key => false));

        State = new InputState(keyboardState);
    }
    
    public InputState State { get; private set; }

    public void Update(InputSnapshot input)
    {
        // Update Keyboard State
        var currentBoardState = UpdateKeyboardState(input);

        State = new InputState(currentBoardState);
    }

    private KeyboardState UpdateKeyboardState(InputSnapshot input)
    {
        var previousKeyStates = State.Keyboard.CurrentKeyStates;
        var currentKeyStates = new Dictionary<Key, bool>(previousKeyStates);
        foreach (var keyEvent in input.KeyEvents.Where(e => !e.Repeat))
        {
            currentKeyStates[keyEvent.Key] = keyEvent.Down;
        }

        var currentBoardState = new KeyboardState(previousKeyStates, currentKeyStates);
        return currentBoardState;
    }
}