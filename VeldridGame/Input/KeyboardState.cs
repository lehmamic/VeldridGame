using Veldrid;

namespace VeldridGame.Input;

public class KeyboardState(IReadOnlyDictionary<Key, bool> previousKeyStates, IReadOnlyDictionary<Key, bool> currentKeyStates)
{
    public IReadOnlyDictionary<Key, bool> PreviousKeyStates { get; } = previousKeyStates;

    public IReadOnlyDictionary<Key, bool> CurrentKeyStates { get; } = currentKeyStates;

    public bool GetKeyValue(Key key)
    {
        return CurrentKeyStates[key];
    }

    public ButtonState GetKeyState(Key key)
    {
        if (PreviousKeyStates[key] == false)
        {
            if (CurrentKeyStates[key] == false)
            {
                return ButtonState.None;
            }
            else
            {
                return ButtonState.Pressed;
            }
        }
        else // Prev state must be 1
        {
            if (CurrentKeyStates[key] == false)
            {
                return ButtonState.Released;
            }
            else
            {
                return ButtonState.Held;
            }
        }
    }
}