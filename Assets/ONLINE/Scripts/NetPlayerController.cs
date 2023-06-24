﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetPlayerController : MonoBehaviourPunCallbacks, IDamageable
{
	[SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, smoothTime;
	[SerializeField] GameObject cameraHolder;
	[SerializeField] GameObject Camera;
	[SerializeField] Item[] items;

	int itemIndex;
	int previousItemIndex = -1;


	bool grounded;
	Vector3 smoothMoveVelocity;
	Vector3 moveAmount;

	Rigidbody rb;
	PhotonView PV;

	const float maxHealth = 100f;
	float currentHealth = maxHealth;

	PlayerManager playerManager;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		PV = GetComponent<PhotonView>();

		playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
	}

	void Start()
	{
		{
			if (PV.IsMine)
			{
				EquipItem(0);
			}
			else
			{
				Camera.SetActive(false);
				Destroy(rb);
			}
		}

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update()
	{
		if (!PV.IsMine)
			return;

		Look();
		Move();


		for (int i = 0; i < items.Length; i++)
		{
			if (Input.GetKeyDown((i + 1).ToString()))
			{
				EquipItem(i);
				break;
			}
		}

		if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
		{
			if (itemIndex >= items.Length - 1)
			{
				EquipItem(0);
			}
			else
			{
				EquipItem(itemIndex + 1);
			}
		}
		else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
		{
			if (itemIndex <= 0)
			{
				EquipItem(items.Length - 1);
			}
			else
			{
				EquipItem(itemIndex - 1);
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
	}

	void Look()
	{
		transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

		cameraHolder.transform.localEulerAngles = Vector3.left;
	}

	void Move()
	{
		Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

		moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
	}

	public void SetGroundedState(bool _grounded)
	{
		grounded = _grounded;
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

		if (PV.IsMine)
		{
			Hashtable hash = new Hashtable();
			hash.Add("itemIndex", itemIndex);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
		}
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if (changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
		{
			EquipItem((int)changedProps["itemIndex"]);
		}
	}

	public void TakeDamage(float damage)
	{
		Debug.Log("took damage" + damage);
		PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
	}

	[PunRPC]
	void RPC_TakeDamage(float damage)
	{
		if (!PV.IsMine)
			return;

		currentHealth -= damage;

		if (currentHealth <= 0)
		{
			Die();
		}
	}

	[SerializeField] float raycastDistance = 0.1f;

	void FixedUpdate()
	{
		if (!PV.IsMine)
			return;

		float rotationY = transform.rotation.eulerAngles.y;
		Quaternion rotation = Quaternion.Euler(0f, rotationY, 0f);

		Vector3 moveDir = rotation * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
		Vector3 moveAmount = moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed) * Time.fixedDeltaTime;

		// Perform collision check
		bool collisionDetected = CheckCollision(moveAmount);

		if (!collisionDetected)
		{
			// Move the player's position
			transform.position += moveAmount;
		}
	}

	bool CheckCollision(Vector3 moveAmount)
	{
		float directionX = Mathf.Sign(moveAmount.x);
		float directionZ = Mathf.Sign(moveAmount.z);

		Vector3 rayOriginBottom = transform.position + new Vector3(directionX, 0f, directionZ) * 0.5f;
		Vector3 rayOriginTop = transform.position + new Vector3(directionX, 1.8f, directionZ) * 0.5f;

		RaycastHit hitBottom;
		RaycastHit hitTop;

		bool collisionDetected = false;

		// Check collision at the bottom of the player
		if (Physics.Raycast(rayOriginBottom, moveAmount, out hitBottom, moveAmount.magnitude + raycastDistance))
		{
			if (!hitBottom.collider.isTrigger)
			{
				collisionDetected = true;
			}
		}

		// Check collision at the top of the player
		if (Physics.Raycast(rayOriginTop, moveAmount, out hitTop, moveAmount.magnitude + raycastDistance))
		{
			if (!hitTop.collider.isTrigger)
			{
				collisionDetected = true;
			}
		}

		return collisionDetected;
	}


	void Die()
	{
		playerManager.Die();
	}
}