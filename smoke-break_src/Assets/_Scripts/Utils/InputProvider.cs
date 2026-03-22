namespace _Scripts.Utils
{
    // Shared InputControls instance to avoid 7+ duplicate allocations.
    public static class InputProvider
    {
        private static InputControls _controls;

        public static InputControls Controls => _controls ??= new InputControls();
    }
}
