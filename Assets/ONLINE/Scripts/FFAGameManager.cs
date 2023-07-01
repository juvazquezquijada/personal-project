using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class FFAGameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public CanvasGroup scoreboard;
    public Camera mapCamera;
    public float gameDuration = 300f; // 5 minutes in seconds
    public TMP_Text timerText;
    public GameObject endGame;
    public AudioSource gameMusicSource;
    public AudioSource lastSecondsMusicSource;
    public float fadeDuration = 1f; // Duration of the fade in seconds




    public static FFAGameManager Instance;

    public bool isGameOver = false;
    private double networkTime = 0f;
    private double syncTime = 0f;

    private bool isTimerInitialized = false;
    private bool isHost = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isHost = true;
            StartGame();
        }
    }

    private void Update()
    {
        if (!isGameOver && isHost)
        {
            double elapsedSeconds = PhotonNetwork.Time - syncTime;
            if (elapsedSeconds >= gameDuration)
            {
                isGameOver = true;
                EndGame();
            }
            else
            {
                double remainingSeconds = gameDuration - elapsedSeconds;
                UpdateTimerText(remainingSeconds);

                // Check if there are 30 seconds remaining
                if (remainingSeconds <= 60 && !lastSecondsMusicSource.isPlaying)
                {
                    // Stop the initial game music and play the 30-second music
                    gameMusicSource.Stop();
                    lastSecondsMusicSource.Play();
                }
            }
        }
        else if (isTimerInitialized)
        {
            double remainingSeconds = gameDuration - (PhotonNetwork.Time - networkTime);
            UpdateTimerText(remainingSeconds);
        }
    }

    private void UpdateTimerText(double remainingSeconds)
    {
        int minutes = Mathf.FloorToInt((float)(remainingSeconds / 60f));
        int seconds = Mathf.FloorToInt((float)(remainingSeconds % 60f));

        // Format the timer text
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void StartGame()
    {
        // Activate the map camera at the start
        mapCamera.gameObject.SetActive(true);

        isGameOver = false;
        networkTime = PhotonNetwork.Time;
        syncTime = networkTime;
        isTimerInitialized = true;
        // Play the initial game music
        gameMusicSource.Play();
    }

    private void EndGame()
    {
        // Notify all clients that the game has ended
        photonView.RPC("GameEnded", RpcTarget.All);

        if (lastSecondsMusicSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic(lastSecondsMusicSource, fadeDuration));
        }
    }

    [PunRPC]
    private void GameEnded()
    {
        isGameOver = true;

        // Show the scoreboard
        scoreboard.alpha = 1;

        Destroy(timerText);
        endGame.gameObject.SetActive(true);

        // Switch to the map camera
        mapCamera.gameObject.SetActive(true);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(networkTime);
        }
        else
        {
            networkTime = (double)stream.ReceiveNext();
            syncTime = PhotonNetwork.Time;
            isTimerInitialized = true;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (isHost && isTimerInitialized)
        {
            // Synchronize the timer with the newly joined player
            photonView.RPC("SyncNetworkTime", newPlayer, networkTime);
        }
    }

    [PunRPC]
    private void SyncNetworkTime(double time)
    {
        networkTime = time;
        syncTime = PhotonNetwork.Time;
        isTimerInitialized = true;
    }

    private IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
    {
        // Store the initial volume of the audio source
        float startVolume = audioSource.volume;

        // Calculate the target volume (0)
        float targetVolume = 0f;

        // Calculate the current time and the end time of the fade
        float currentTime = 0f;
        float endTime = currentTime + duration;

        // Gradually decrease the volume over time
        while (currentTime < endTime)
        {
            // Calculate the current volume based on the fade progress
            float t = currentTime / duration;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);

            // Increment the current time
            currentTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the volume is set to the target volume
        audioSource.volume = targetVolume;

        // Stop the audio source
        audioSource.Stop();
    }

}
