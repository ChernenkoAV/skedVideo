namespace skedVideo.Player
{
    public class PlayerStatus
    {
        public string FilePath { get; set; }
        public StateKind State { get; set; }
    }

    public enum StateKind
    {
        Stop = 0,
        Pause = 1,
        Play = 2
    }
}