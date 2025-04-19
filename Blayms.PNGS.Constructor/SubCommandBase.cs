namespace Blayms.PNGS.Constructor
{
    public class SubCommandBase<TCommand> : CommandBase where TCommand : CommandBase
    {
        private TCommand? m_ActualOwner = null;
        public TCommand ActualOwner
        {
            get
            {
                if(m_ActualOwner == null)
                {
                    m_ActualOwner = (TCommand)Owner!;
                }
                return m_ActualOwner;
            }
        }
        public SubCommandBase(TCommand? actualOwner) : base(actualOwner)
        {
            m_ActualOwner = actualOwner;
        }
    }
}
