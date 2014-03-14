using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace StillebenBrowser
{
    enum SwipeDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    class LeapListener : Listener
    {
        int handsLastFrame = 0;

        public override void OnConnect(Controller leapController)
        {         
            leapController.Config.SetFloat("Gesture.Swipe.MinLength", 10);
            leapController.Config.SetFloat("Gesture.Swipe.MinVelocity", 100);
            leapController.Config.Save();
            leapController.EnableGesture(Gesture.GestureType.TYPESWIPE);
        }

        public override void OnFrame(Controller leapController)
        {
            Frame currentFrame = leapController.Frame();

            if (handsLastFrame == 0 && currentFrame.Hands.Count > 0 && LeapRegisterFingers != null)
                LeapRegisterFingers(true);
            else if (handsLastFrame > 0 && currentFrame.Hands.Count == 0 && LeapRegisterFingers != null)
                LeapRegisterFingers(false);
            handsLastFrame = currentFrame.Hands.Count;

            if (currentFrame.Hands.Count > 0 &&
                currentFrame.Hands[0].Fingers.Count > 0 && 
                LeapSwipe != null)
            {
                GestureList gestures = currentFrame.Gestures();
                foreach (Gesture gesture in gestures)
                {
                    SwipeGesture swipe = new SwipeGesture(gesture);
                    if (Math.Abs(swipe.Direction.x) > Math.Abs(swipe.Direction.y)) // Horizontal swipe
                    {
                        if (swipe.Direction.x > 0)
                            LeapSwipe(SwipeDirection.Right);
                        else
                            LeapSwipe(SwipeDirection.Left);
                    }
                    else // Vertical swipe
                    {
                        if (swipe.Direction.y > 0)
                            LeapSwipe(SwipeDirection.Up);
                        else
                            LeapSwipe(SwipeDirection.Down);
                    }
                }
            }
        }

        public delegate void SwipeEvent(SwipeDirection sd);
        public event SwipeEvent LeapSwipe;

        public delegate void RegisterFingers(bool connected);
        public event RegisterFingers LeapRegisterFingers;
    }
}
