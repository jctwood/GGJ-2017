﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrabController : MonoBehaviour 
{
	public int currentWave = 0;
	public Transform[] waves;
	public KeyCode upKey = KeyCode.UpArrow;
	public KeyCode downKey = KeyCode.DownArrow;
	public KeyCode trickKey = KeyCode.Space;
	public float elapsedTime = 0.0f;
	public float bobSpeed = 2.0f;
	public float bobAmount = 0.2f;
	public float rotateSpeed = 2.0f;
	public float rotateAmount = 20.0f;
	public float rotationOffset = 90.0f;
	public float crabHorizontalPosition = 0.0f;
	private Vector3 crabPosition;

	public float jumpHeight = 2.0f;
	public float jumpDuration = 0.5f;
	private float jumpTime = 0.0f;
	private bool jumping = false;
	private Vector3 jumpStartPosition, jumpTargetPosition;

	private int score = 0;
	public TextMesh scoreText;
    public List<GameObject> backBottels;
    public int bottleCount = 0;

	float touchStart = 0.0f;

	private Vector3 trickVector;
	public float trickDuration = 0.2f;
	public Quaternion trickStartRotation;
	private float trickTime = 0.0f;
	private bool tricking = false;

	private bool stunned = true;
	public float stunDuration = 1.0f;
	private float stunTime = 0.0f;

	private bool dying = false;
	public float deathDuration = 1.0f;
	public float deathDelay = 1.0f;
	public float deathEndDelay = 1.0f;
	private float deathTime = 0.0f;
	private Quaternion deathStartRotation;
	private Vector3 deathStartPosition;
	private bool playedDeathSound = false;
	public AudioSource mainMusic;

	public TextMesh tweetText;
	private float tweetTime = 0.0f;
	public float tweetDuration = 10.0f;

	public string[] recentTweets = {	"have a crabtastic day #crabtastic",
								"malcolm has a crabtastic moustache",
								"did you hear about the crab that went surfing? \nit pulled a mussel #crabtastic"};

    AudioSource audioSource;
    public AudioClip bottleCollect;
    public AudioClip[] splashNoise;
    public AudioClip succsesTrick;
    public AudioClip failTrick;

	void Start () 
	{
        audioSource = GetComponent<AudioSource>();

		if (waves.Length == 0)
		{
			Debug.Log("Add wave transforms to character controller bottom to top!");
		}
        
		crabPosition = waves[currentWave].transform.position;
        
        for(int i = 0; i < backBottels.Count; i++)
        {
            backBottels[i].GetComponent<MeshRenderer>().enabled = false;
        }
        
		UpdateMovement();
	}

	void Update () 
	{
		elapsedTime += Time.deltaTime;

		if (tweetText.text != "")
		{
			tweetTime += Time.deltaTime;

			if (tweetTime > tweetDuration)
			{
				tweetText.text = "";
			}
		}

		if (!dying)
		{
			HandleInput();

			if (stunned)
			{
				stunTime += Time.deltaTime;

				if (stunTime > stunDuration)
				{
					stunned = false;
				}
			}
			else
			{
				UpdateMovement();
			}
		}
		else
		{
			deathTime += Time.deltaTime;

			if (deathTime > deathDelay)
			{
				if ((deathTime - deathDelay) < deathDuration)
				{
					float t = (deathTime - deathDelay) / deathDuration;
					float crabHeight = Mathf.Sin(t * Mathf.PI * 1.5f) * jumpHeight * 2;

					Vector3 newPosition = deathStartPosition;
					newPosition.y = crabHeight;
					transform.position = newPosition;

					Quaternion newTrickRotation = Quaternion.AngleAxis(180 * t, new Vector3(1, 0, 0));
					transform.rotation = deathStartRotation * newTrickRotation;

					if (!playedDeathSound)
					{
						int splashIndex = Random.Range(0, 3);

						LoadAndPlay(splashNoise[splashIndex]);
						playedDeathSound = true;
					}
				}
				else if ((deathTime - deathDelay - deathDuration) > deathEndDelay)
				{
					SceneManager.LoadScene("StartScene");
				}
			}
		}
	}

	void UpdateMovement()
	{
		Vector3 newPosition = new Vector3();

		float currentBob = Mathf.Cos(elapsedTime * bobSpeed) * bobAmount;
		transform.eulerAngles = new Vector3(0.0f, 0.0f, rotationOffset + Mathf.Sin(elapsedTime * rotateSpeed) * rotateAmount);
	
		if (jumping)
		{
			jumpTime += Time.deltaTime;

			if (jumpTime > jumpDuration)
			{
				jumping = false;
				crabPosition = jumpTargetPosition;
			}

			if (tricking)
			{
				if (jumping == false)
				{
					tricking = false;
					transform.rotation = trickStartRotation;
					stunned = true;
					stunTime = 0.0f;
					ClearTheBottels();
                    LoadAndPlay(failTrick);
				}
				else
				{
					trickTime += Time.deltaTime;

					if (trickTime > trickDuration)
					{
						// Successful trick!
						tricking = false;
                        LoadAndPlay(succsesTrick);
                    }
					else
					{
						Quaternion newTrickRotation = Quaternion.AngleAxis(360 * trickTime / trickDuration, trickVector);
						transform.rotation = trickStartRotation * newTrickRotation;
					}
				}
			}

			float t = jumpTime / jumpDuration;
			float crabHeight = Mathf.Sin(t * Mathf.PI) * jumpHeight;
			Vector3 jumpPosition = Vector3.Lerp(jumpStartPosition, jumpTargetPosition, t);

			newPosition = jumpPosition + new Vector3(0.0f, crabHeight, 0.0f);
		}
		else
		{
			newPosition = crabPosition + new Vector3(0.0f, currentBob, 0.0f);
		}

		newPosition.x = crabHorizontalPosition;
		transform.position = newPosition;
	}

	void HandleInput()
	{
		if (!jumping)
		{
			if (Input.GetKey(upKey) && currentWave < (waves.Length - 1))
			{
				currentWave += 1;
				Jump();
			}
			else if (Input.GetKey(downKey) && currentWave > 0)
			{
				currentWave -= 1;
				Jump();
			}
			else if (Input.GetKeyDown(trickKey))
			{
				Jump();
			}

			if (Input.touchCount > 0)
			{
				Touch touch = Input.GetTouch(0);

				if (touch.phase == TouchPhase.Began)
				{
					touchStart = touch.position.y;
				}
				else if (touch.phase == TouchPhase.Ended)
				{
					if (touch.position.y - touchStart > 10.0f && currentWave < (waves.Length - 1))
					{
						currentWave += 1;
						Jump();
					}
					else if (touch.position.y - touchStart < -10.0f  && currentWave > 0)
					{
						currentWave -= 1;
						Jump();
					}
					else
					{
						Jump();
					}
				}
			}
		}
		else if (jumping && !tricking)
		{
			if (Input.GetKeyDown(trickKey))
			{
				Trick();
			}

			if (Input.touchCount > 0)
			{
				if (Input.GetTouch(0).phase == TouchPhase.Ended)
				{
					Trick();
				}
			}
		}
	}

	void Jump()
	{
		jumping = true;
		jumpTime = 0.0f;
		jumpStartPosition = crabPosition;
		jumpTargetPosition = waves[currentWave].transform.position;
	}

	void Trick()
	{
		tricking = true;
		trickTime = 0.0f;
		trickStartRotation = transform.rotation;
		trickVector = Random.insideUnitSphere;
	}

	void Die()
	{
		dying = true;
		deathTime = 0.0f;
		deathDelay = 0.0f;
		deathStartRotation = transform.rotation;
		deathStartPosition = transform.position;

		mainMusic.volume = 0.0f;
		LoadAndPlay(failTrick);

		WaveObject[] waveObjects = GameObject.FindObjectsOfType<WaveObject>();

		foreach (WaveObject waveObject in waveObjects)
		{
			waveObject.horizontalSpeed = 0.0f;
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.tag == "Score")
		{
			

			if (score > PlayerPrefs.GetInt("Highscore"))
			{
				PlayerPrefs.SetInt("Highscore", score);
			}

			scoreText.text = score.ToString();
            collider.GetComponent<MeshRenderer>().enabled = false;
			ShowAnotherBottle();
			LoadAndPlay(bottleCollect);

            score += bottleCount;
        }

		if (collider.tag == "Crabtastic")
		{
            ShowAnotherBottle();
            score += bottleCount;
            LoadAndPlay(bottleCollect);
            if (score > PlayerPrefs.GetInt("Highscore"))
			{
				PlayerPrefs.SetInt("Highscore", score);
			}

			scoreText.text = score.ToString();
			collider.GetComponent<MeshRenderer>().enabled = false;
			

			ShowTweet();
		}

		if(collider.tag == "Enemy" && !dying)
        {
            ClearTheBottels();
			Die();
        }
    }

    void ShowAnotherBottle()
    {
        if(bottleCount < backBottels.Count)
        {
            backBottels[bottleCount].GetComponent<MeshRenderer>().enabled = true;

            bottleCount++;
        }
    }

    void ClearTheBottels()
    {
        for (int i = 0; i < backBottels.Count; i++)
        {
            backBottels[i].GetComponent<MeshRenderer>().enabled = false;
        }

		bottleCount = 0;
    }

	void ShowTweet()
	{
		int randomTweet = Random.Range(0, recentTweets.Length);
		tweetText.text = recentTweets[randomTweet];

		tweetTime = 0.0f;
	}

    void LoadAndPlay(AudioClip audioclip)
    {
        audioSource.clip = audioclip;
        audioSource.Play();
    }
}