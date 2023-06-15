using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TitleScreenManager : MonoBehaviour
{

    public GameObject startButton;
    public Button multiplayerButton;
    public GameObject mapSelect;
    public GameObject firstMapButton;
    public GameObject tutorialText;
    public Button tutorialButton;
    public AudioSource audioSource;
    public AudioClip selectSound;
    public AudioClip backSound;
    public GameObject loadingText;
    private bool isMenuActive = false;
    
    

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        isMenuActive = true;
        tutorialText.gameObject.SetActive(false);
        tutorialButton.gameObject.SetActive(true);
        mapSelect.gameObject.SetActive(false);
        startButton.gameObject.SetActive(true);
        multiplayerButton.gameObject.SetActive(true);
    }

   void Update()
{
    if (Input.GetButtonDown("Start"))
    {
        StartGame();
    }
    if (isMenuActive)
    {
        // Check for D-pad or arrow key input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (verticalInput > 0 || horizontalInput < 0)
        {
            // Move up or left in the menu
            EventSystem.current.SetSelectedGameObject(EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp().gameObject);
            audioSource.PlayOneShot(selectSound);
        }
        else if (verticalInput < 0 || horizontalInput > 0)
        {
            // Move down or right in the menu
            EventSystem.current.SetSelectedGameObject(EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown().gameObject);
            audioSource.PlayOneShot(selectSound);
        }
    }
}
    public void StartGame()
    {
        mapSelect.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);
        audioSource.PlayOneShot(selectSound);
        tutorialButton.gameObject.SetActive(false);
        multiplayerButton.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(firstMapButton);
    }

   
    public void StartMultiplayer()
    {
        SceneManager.LoadScene("MPMenu");
    }
    public void StartWarehouse() // starts the warehouse map
    {
        SceneManager.LoadScene("Warehouse");
        audioSource.PlayOneShot(selectSound);
        loadingText.gameObject.SetActive(true);
    }
    public void StartCityDay() // starts the CityDay map
    {
        SceneManager.LoadScene("CityDay");
        audioSource.PlayOneShot(selectSound);
        loadingText.gameObject.SetActive(true);
    }
    public void StartCityNight()// starts the CityNight map
    {
        SceneManager.LoadScene("CityNight");
        audioSource.PlayOneShot(selectSound);
        loadingText.gameObject.SetActive(true);
    }
    public void StartStore()// starts the store map
    {
        SceneManager.LoadScene("Store");
        audioSource.PlayOneShot(selectSound);
        loadingText.gameObject.SetActive(true);
    }
    public void StartMPArena()
    {
        SceneManager.LoadScene("MPArena");
        audioSource.PlayOneShot(selectSound);
        loadingText.gameObject.SetActive(true);
    }

    public void BackToTitle()
    {
        mapSelect.gameObject.SetActive(false);
        tutorialText.gameObject.SetActive(false);
        startButton.gameObject.SetActive(true);
        audioSource.PlayOneShot(backSound);
        tutorialButton.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(startButton);
        multiplayerButton.gameObject.SetActive(true);
    }

    public void ShowTutorial()
    {
        tutorialText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);
        audioSource.PlayOneShot(selectSound);
        tutorialButton.gameObject.SetActive(false);
    }
}

