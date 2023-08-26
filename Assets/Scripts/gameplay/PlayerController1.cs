using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController1 : MonoBehaviour
{
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, smoothTime, yMouseSensitivity;
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject cameraHolder, weaponHolder;
    [SerializeField] float jumpForce;
    [SerializeField] Item[] items;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Image healthBar;
    [SerializeField] TMP_Text healthbarText;
    [SerializeField] GameObject wepCamera;
    [SerializeField] Animator animator;

    public TextMeshProUGUI health, score; //health indicator
    public AudioClip pauseSound;
    public AudioClip hurtSound;
    public AudioClip jumpSound;
    public bool isJumping = false;
    public bool isPaused = false;
    public bool isDead = false;
    public GameObject pauseMenuPanel;
    int itemIndex;
    int previousItemIndex = -1;
    float verticalLookRotation;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 10f;
    public float sprintStaminaCost = 20f;
    public float jumpStaminaCost = 10f;
    public float refillDelayDuration = 3f;
    private bool isRefillingStamina;
    private float timeSinceLastAction;
    private bool isSprinting;

    public Image staminaBarImage;

    CharacterController characterController;

    const float maxHealth = 100f;
    float currentHealth = maxHealth;
    public GameObject lowHealthText;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        EquipItem(0);
        animator.SetTrigger("SwitchWeapon");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina;
        timeSinceLastAction = Time.time;
    }

    void Update()
    {
        if (isDead || isPaused)
            return;

            Look();
            Move();

            for (int i = 0; i < items.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    EquipItem(i);
                    animator.SetTrigger("SwitchWeapon");
                    break;
                }
            }

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                // Scroll up
                if (itemIndex < items.Length - 1)
                {
                    EquipItem(itemIndex + 1);
                    animator.SetTrigger("SwitchWeapon");
                 }
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                // Scroll down
                if (itemIndex > 0)
                {
                    EquipItem(itemIndex - 1);
                    animator.SetTrigger("SwitchWeapon");
                 }
            }

            if (Input.GetMouseButton(0))
            {
                items[itemIndex].Use();
            }

            if (transform.position.y < -5f) // Die if you fall out of the world
            {
                Die();
            }

            if (characterController.isGrounded)
            {
                // Reset jumping flag
                isJumping = false;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Call the Reload() method on the currently equipped gun
                if (items[itemIndex] is Gun equippedGun)
                {
                    equippedGun.Reload();
                }
            }

    

        if (currentHealth <= 30)
        {
            lowHealthText.gameObject.SetActive(true);
        }

        if (isDead == true)
        {
            moveAmount = Vector3.zero; // stop the movement
            SpawnManager.Instance.GameOver();
            return; // exit the method
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }

    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * yMouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
        weaponHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void Move()
    {
        float rotationY = transform.rotation.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0f, rotationY, 0f);

        Vector3 moveDir = rotation * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (characterController.isGrounded)
        {
            // Check for sprint input and adjust movement speed and stamina
            if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
            {
                isSprinting = true;
                moveAmount = moveDir * sprintSpeed;
                currentStamina -= sprintStaminaCost * Time.deltaTime;
                timeSinceLastAction = Time.time;
                animator.SetTrigger("PlayerBob");
            }
            else
            {
                isSprinting = false;
                moveAmount = moveDir * walkSpeed;
                animator.ResetTrigger("PlayerBob");
            }

            // Check for jump input and consume stamina
            if (Input.GetButton("Jump") && currentStamina >= jumpStaminaCost)
            {
                isJumping = true;
                moveAmount.y = jumpForce;
                currentStamina -= jumpStaminaCost;
                timeSinceLastAction = Time.time;
                audioSource.PlayOneShot(jumpSound);
            }
        }
        else
        {
            moveAmount.y += Physics.gravity.y * Time.deltaTime;

            if (moveAmount.y < 0)
            {
                isJumping = false;
            }
        }

        // Prevent sprinting when stamina is 0
        if (currentStamina <= 0)
        {
            isSprinting = false;
        }

        // Refill stamina if it has been refillDelayDuration seconds since the last action
        if (Time.time - timeSinceLastAction >= refillDelayDuration)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }

        // Update stamina bar fill amount
        staminaBarImage.fillAmount = currentStamina / maxStamina;

        // Apply movement
        characterController.Move(moveAmount * Time.deltaTime);
    }
    void EquipItem(int _index)
    {
        if (_index == previousItemIndex)
            return;

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        // Call the UpdateAmmoUIOnSwitch method on the currently equipped gun (if it is a SingleShotGun)
        if (items[itemIndex] is PlayerGun playerGun)
        {
            playerGun.UpdateAmmoUIOnSwitch();
        }
    }

    void PlayWeaponSwitch()
    {
        animator.SetTrigger("SwitchWeapon");
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player is Dead!");
    }

    public void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f; // Pause the game
        if (!isPaused)
        {
            // Show pause menu

            isPaused = true;
            pauseMenuPanel.SetActive(true);
            audioSource.PlayOneShot(pauseSound);
        }
        else
        {
            // Hide pause menu
            Time.timeScale = 1f; // Unpause the game
            isPaused = false;
            pauseMenuPanel.SetActive(false);
            Cursor.visible = false;
        }
    }

    public void ResumeGame()
    {
        // Unpause the game
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuPanel.SetActive(false);

        // Enable the camera and hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    public void QuitGame()
    {
        SceneManager.LoadScene("TitleScene");
        AudioListener.pause = false; // Resume the music
        Time.timeScale = 1f;
    }

    void UpdateHealthUI()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
        healthbarText.text = currentHealth.ToString("F1"); // Formats to one decimal place
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Health"))
        {
            currentHealth += 25;
            if (currentHealth > 100) currentHealth = 100;
            Destroy(other.gameObject);
            UpdateHealthUI();
        }

        else if (other.gameObject.CompareTag("Fireball"))
        {
            currentHealth -= 6;
            if (currentHealth < 0) currentHealth = 0;
            Destroy(other.gameObject);
            audioSource.PlayOneShot(hurtSound);
            UpdateHealthUI();
        }
        else if (other.gameObject.CompareTag("Demon"))
        {
            currentHealth -= 5;
            if (currentHealth < 0) currentHealth = 0;
            audioSource.PlayOneShot(hurtSound);
            UpdateHealthUI();
        }
        else if (other.gameObject.CompareTag("Zombie"))
        {
            currentHealth -= 5;
            if (currentHealth < 0) currentHealth = 0;
            audioSource.PlayOneShot(hurtSound);
            UpdateHealthUI();
        }
        else if (other.gameObject.CompareTag("Soldier"))
        {
            currentHealth -= 10;
            if (currentHealth < 0) currentHealth = 0;
            audioSource.PlayOneShot(hurtSound);
            UpdateHealthUI();
        }
        else if (other.gameObject.CompareTag("EnemyProjectile"))
        {
            currentHealth -= 7;
            if (currentHealth < 0) currentHealth = 0;
            audioSource.PlayOneShot(hurtSound);
            UpdateHealthUI();
        }
        else if (other.gameObject.CompareTag("EnemyRocket"))
        {
            currentHealth -= 30;
            if (currentHealth < 0) currentHealth = 0;
            Destroy(other.gameObject);
            audioSource.PlayOneShot(hurtSound);
            UpdateHealthUI();
        }
    }
}
