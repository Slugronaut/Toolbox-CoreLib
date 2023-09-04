using Peg.AutoCreate;
using UnityEngine;

namespace Peg.Systems
{
    /// <summary>
    /// Base class for deriving core Game class that performs any vital intialization
    /// and setup functionalities throughout the lifetime of the game.
    /// </summary>
    /// <remarks>
    /// The AutoAwake() method will invoke a buffered/pending GameInitialized message.
    /// </remarks>
    [AutoCreate]
    public class GameBootEvents
    {
        GameInitializedEvent BufferedMsg;
        

        /// <summary>
        /// Make sure this is called at the end of your overriden 'Start' method!
        /// </summary>
        protected virtual void AutoStart()
        {
            BufferedMsg = new GameInitializedEvent();
            GlobalMessagePump.Instance.PostMessage(BufferedMsg);
        }

        /// <summary>
        /// For the love of God do not forget to call the base method if you override!
        /// </summary>
        protected virtual void AutoDestroy()
        {
            if (Application.isPlaying)
            {
                //for some reason, this message can be null by this point?
                if(BufferedMsg != null) GlobalMessagePump.Instance.RemoveBufferedMessage(BufferedMsg);
                GlobalMessagePump.Instance.PostMessage(new GameShuttingdownEvent());
            }
        }

    }
}

namespace Peg
{
    public class GameInitializedEvent : IBufferedMessage, IDeferredMessage { }
    public class GameShuttingdownEvent : IMessageEvent { }
}