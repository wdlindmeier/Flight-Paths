using System.Collections.Generic;

namespace UnityEngine.Experimental.Input
{
    public class InputHandlerNode : IInputHandler
    {
        List<IInputHandler> m_Children = new List<IInputHandler>();
        public List<IInputHandler> children { get { return m_Children; } }

        public InputEventHandler handler { get; set; }

        public bool ProcessEvent(InputEvent inputEvent)
        {
            if (handler != null && handler(inputEvent))
                return true;

            for (int i = 0; i < m_Children.Count; i++)
            {
                if (m_Children[i].ProcessEvent(inputEvent))
                    return true;
            }
            return false;
        }

        public void BeginUpdate()
        {
            for (int i = 0; i < m_Children.Count; i++)
            {
                m_Children[i].BeginUpdate();
            }
        }

        public void EndUpdate()
        {
            for (int i = 0; i < m_Children.Count; i++)
            {
                m_Children[i].EndUpdate();
            }
        }
    }
}
