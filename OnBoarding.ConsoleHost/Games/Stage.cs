using Libplanet.Crypto;
using OnBoarding.ConsoleHost.Games.Serializations;

namespace OnBoarding.ConsoleHost.Games;

sealed class Stage
{
    private readonly Player _player;
    private readonly MonsterCollection _monsters;
    private readonly Character[] _characters;

    public Stage(StageInfo stageInfo, int seed, TextWriter @out)
    {
        _player = new(stageInfo.Player);
        _monsters = new(stageInfo.Monsters);
        _characters = new Character[] { _player }.Concat(_monsters).ToArray();
        Random = new(seed);
        Out = @out;
    }

    public Stage(StageInfo stageInfo, int seed)
        : this(stageInfo, seed, new StringWriter())
    {
    }

    public Address Address { get; } = new PrivateKey().ToAddress();

    public Player Player => _player;

    public MonsterCollection Monsters => _monsters;

    public long Turn { get; private set; }

    public bool IsEnded => _player.IsDead == true || _monsters.AliveCount == 0;

    public Random Random { get; }

    public TextWriter Out { get; }

    public IEnumerable<Character> Characters => _characters;

    public bool IsPlaying { get; private set; }

    public void Play()
    {
        if (IsEnded == true)
            throw new InvalidOperationException("Stage has already ended.");
        if (IsPlaying == true)
            throw new InvalidOperationException("Stage is playing.");

        using var _ = new PlayScope(this);
        Turn = 0;
        while (IsEnded == false)
        {
            Update();
            Turn++;
        }
    }

    public async Task PlayAsync(int tick, CancellationToken cancellationToken)
    {
        if (IsEnded == true)
            throw new InvalidOperationException("Stage has already ended.");
        if (IsPlaying == true)
            throw new InvalidOperationException("Stage is playing.");

        using var _ = new PlayScope(this);
        Turn = 0;
        while (cancellationToken.IsCancellationRequested == false && IsEnded == false)
        {
            await Out.WriteLineAsync($"Turn #{Turn}");
            Update();
            Turn++;
            await Task.Delay(tick, cancellationToken: default);
        }
        if (cancellationToken.IsCancellationRequested == true)
        {
            throw new TaskCanceledException("Play has been canceled.");
        }
    }

    private void Update()
    {
        foreach (var item in _characters)
        {
            UpdateSkills(item);
        }
    }

    private void UpdateSkills(Character character)
    {
        foreach (var item in character.Skills)
        {
            if (item.CanExecute(this) == true)
            {
                item.Execute(this);
            }
            item.Tick();
        }
    }

    #region PlayScope

    sealed class PlayScope : IDisposable
    {
        private readonly Stage _stage;

        public PlayScope(Stage stage)
        {
            _stage = stage;
            _stage.IsPlaying = true;
        }

        public void Dispose()
        {
            _stage.IsPlaying = false;
        }
    }

    #endregion
}
