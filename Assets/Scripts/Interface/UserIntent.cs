using System;

namespace Bitwise.Interface
{
    public enum IntentTarget
    {
        None = 0,
        Commands
    }

    public abstract class UserIntent
    {
        public enum SourceType
        {
            Text
        }

        public enum IntentType
        {
            Query,
            Quit,
            Confirm,
            Cancel,
            Unknown,
            Debug,
            Diag,
            Reboot
        }

        public SourceType Source { get; private set; }
        public IntentType Intent { get; private set; }
        public IntentTarget Target { get; private set; }

        protected UserIntent(SourceType source, IntentType intent, IntentTarget target = IntentTarget.None)
        {
            Source = source;
            Intent = intent;
            Target = target;
        }
    }

    public class TextIntent : UserIntent
    {
        public TextIntent(IntentType intent, IntentTarget target = IntentTarget.None) : base(SourceType.Text, intent, target) { }
    }
}