public class AsyncState
{
    private AsyncStateEnum _State;

    public bool IsCompleted => _State == AsyncStateEnum.Completed;

    public void Complete()
    {
        _State = AsyncStateEnum.Completed;
    }

    public AsyncState()
    {
        _State = AsyncStateEnum.Running;
    }
}

public enum AsyncStateEnum
{
    Running,
    Completed
}
