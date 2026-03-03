using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public sealed class VideoUIController
{
    private readonly MonoBehaviour host;
    private readonly VideoPlayer videoPlayer;

    private readonly Button playButton, pauseButton, muteButton, unmuteButton, replayButton;
    private readonly ProgressBar progressBar;

    private readonly System.Func<bool> isVideoScreenVisible;

    private Coroutine progressRoutine;
    private static readonly WaitForSeconds Wait = new(0.1f);

    public VideoUIController(
        MonoBehaviour host,
        VideoPlayer videoPlayer,
        Button playButton,
        Button pauseButton,
        Button muteButton,
        Button unmuteButton,
        Button replayButton,
        ProgressBar progressBar,
        System.Func<bool> isVideoScreenVisible)
    {
        this.host = host;
        this.videoPlayer = videoPlayer;

        this.playButton = playButton;
        this.pauseButton = pauseButton;
        this.muteButton = muteButton;
        this.unmuteButton = unmuteButton;
        this.replayButton = replayButton;
        this.progressBar = progressBar;

        this.isVideoScreenVisible = isVideoScreenVisible;
    }

    public void Hook()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted += OnPrepared;
    }

    public void Unhook()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted -= OnPrepared;
    }

    public void Enter()
    {
        if (videoPlayer != null && !videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
            if (playButton != null) playButton.SetEnabled(false);
        }

        RefreshButtonStates();
        StartProgress();
    }

    public void Leave()
    {
        StopProgress();
    }

    public void Play()
    {
        if (videoPlayer == null) return;
        videoPlayer.Play();
        SetPlayingUI(true);
    }

    public void Pause()
    {
        if (videoPlayer == null) return;
        videoPlayer.Pause();
        SetPlayingUI(false);
    }

    public void Mute()
    {
        if (videoPlayer == null) return;
        videoPlayer.SetDirectAudioMute(0, true);
        SetMutedUI(true);
    }

    public void Unmute()
    {
        if (videoPlayer == null) return;
        videoPlayer.SetDirectAudioMute(0, false);
        SetMutedUI(false);
    }

    public void Replay()
    {
        if (videoPlayer == null) return;

        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
            return;
        }

        videoPlayer.time = 0;
        if (videoPlayer.frameCount > 0) videoPlayer.frame = 0;

        videoPlayer.Play();
        SetPlayingUI(true);
    }

    private void OnPrepared(VideoPlayer _)
    {
        if (playButton != null)
            playButton.SetEnabled(true);
    }

    private void RefreshButtonStates()
    {
        if (videoPlayer == null) return;
        SetPlayingUI(videoPlayer.isPlaying);
        SetMutedUI(videoPlayer.GetDirectAudioMute(0));
    }

    private void SetPlayingUI(bool playing)
    {
        SetVisible(playButton, !playing);
        SetVisible(pauseButton, playing);
    }

    private void SetMutedUI(bool muted)
    {
        SetVisible(muteButton, !muted);
        SetVisible(unmuteButton, muted);
    }

    private static void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void StartProgress()
    {
        if (host == null) return;
        StopProgress();
        progressRoutine = host.StartCoroutine(ProgressLoop());
    }

    private void StopProgress()
    {
        if (host == null) return;
        if (progressRoutine != null) host.StopCoroutine(progressRoutine);
        progressRoutine = null;
    }

    private IEnumerator ProgressLoop()
    {
        while (isVideoScreenVisible != null && isVideoScreenVisible())
        {
            if (progressBar != null && videoPlayer != null && videoPlayer.isPrepared && videoPlayer.length > 0.0001)
                progressBar.value = (float)(videoPlayer.time / videoPlayer.length) * 100f;

            yield return Wait;
        }
    }
}