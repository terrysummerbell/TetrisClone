using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour
{

    // reference to our board
    private Board m_gameBoard;

    // reference to our spawner
    private Spawner m_spawner;

    private SoundManager m_soundManager;

    private ScoreManager m_scoreManager;

    // currently active shape
    private Shape m_activeShape;

    // ghost for visualisation
    Ghost m_ghost;

    public float m_dropInterval = 0.9f;

    private float m_dropIntervalModded;

    private float m_timeToDrop;

    /*
    private float m_timeToNextKey;

    [Range(0.02f, 1f)]
    public float m_keyRepeatRate = 0.25f;
    */

    float m_timeToNextKeyLeftRight;


    [Range(0.02f, 1f)]
    public float m_keyRepeatRateLeftRight = 0.25f;

    float m_timeToNextKeyDown;

    [Range(0.01f, 1f)]
    public float m_keyRepeatRateDown = 0.25f;

    float m_timeToNextKeyRotate;

    [Range(0.02f, 1f)]
    public float m_keyRepeatRateRotate = 0.25f;

    public GameObject m_gameOverPanel;

    bool m_gameOver = false;

    public IconToggle m_rotIconToggle;

    bool m_clockwise = true;

    public bool m_isPaused = false;

    public GameObject m_pausePanel;


    // Start is called before the first frame update
    void Start()
    {
   
        // find spawner and board with generic version of GameObject.FindObjectOfType, slower but less typing
        m_gameBoard = GameObject.FindObjectOfType<Board>();
        m_spawner = GameObject.FindObjectOfType<Spawner>();
        m_soundManager = GameObject.FindObjectOfType<SoundManager>();
        m_scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        m_ghost = GameObject.FindObjectOfType<Ghost>();

        m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;
        m_timeToNextKeyDown = Time.time + m_keyRepeatRateDown;
        m_timeToNextKeyRotate = Time.time + m_keyRepeatRateRotate;


        if (!m_gameBoard)
        {
            Debug.LogWarning("WARNING! There is no game board defined!");
        }

        if (!m_soundManager)
        {
            Debug.LogWarning("WARNING! There is no sound manager defined!");
        }

        if (!m_scoreManager)
        {
            Debug.LogWarning("WARNING! There is no score manager defined!");
        }

        if (!m_spawner)
        {
            Debug.LogWarning("WARNING! There is no spawner defined!");
        }
        else
        {
            m_spawner.transform.position = Vectorf.Round(m_spawner.transform.position);

            if (!m_activeShape)
            {
                m_activeShape = m_spawner.SpawnShape();
            }
        }
        if (m_gameOverPanel)
        {
            m_gameOverPanel.SetActive(false);
        }

        if (m_pausePanel)
        {
            m_pausePanel.SetActive(false);
        }

        m_dropIntervalModded = m_dropInterval;

    }


    // Update is called once per frame
    void Update()
    {
        // if we are missing a spawner or game board or active shape, then we don't do anything
        if (!m_gameBoard || !m_spawner || !m_activeShape || m_gameOver || !m_soundManager || !m_scoreManager)
        {
            return;
        }

        PlayerInput();
    }


    private void LateUpdate()
    {
        if (m_ghost)
        {
            m_ghost.DrawGhost(m_activeShape, m_gameBoard);
        }
    }


    /*
     ** PLAYER INPUT CONTROLS **
    */

    void PlayerInput()
    {
        // example of NOT usiung the Input Manager
        // if (Input.GetKey("right") && (Time.time > m_timeToNextKey) || Input.GetKeyDown (KeyCode.RightArrow))

        if ((Input.GetButton("MoveRight") && (Time.time > m_timeToNextKeyLeftRight)) || Input.GetButtonDown("MoveRight"))
        {
            m_activeShape.MoveRight();
            m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;

            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                m_activeShape.MoveLeft();
                PlaySound(m_soundManager.m_errorSound, 0.5f);
            }
            else
            {
                PlaySound(m_soundManager.m_moveSound, 0.5f);
            }

        }
        else if ((Input.GetButton("MoveLeft") && (Time.time > m_timeToNextKeyLeftRight)) || Input.GetButtonDown("MoveLeft"))
        {
            m_activeShape.MoveLeft();
            m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;

            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                m_activeShape.MoveRight();
                PlaySound(m_soundManager.m_errorSound, 0.5f);
            }
            else
            {
                PlaySound(m_soundManager.m_moveSound, 0.5f);
            }

        }
        else if (Input.GetButtonDown("Rotate") && (Time.time > m_timeToNextKeyRotate))
        {
            // m_activeShape.RotateRight();
            m_activeShape.RotateClockwise(m_clockwise);

            m_timeToNextKeyRotate = Time.time + m_keyRepeatRateRotate;

            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                // m_activeShape.RotateLeft();
                m_activeShape.RotateClockwise(!m_clockwise);

                PlaySound(m_soundManager.m_errorSound, 0.5f);
            }
            else
            {
                PlaySound(m_soundManager.m_moveSound, 0.5f);
            }
        }
        else if (Input.GetButton("MoveDown") && (Time.time > m_timeToNextKeyDown) || (Time.time > m_timeToDrop))
        {
            m_timeToDrop = Time.time + m_dropIntervalModded;
            m_timeToNextKeyDown = Time.time + m_keyRepeatRateDown;

            m_activeShape.MoveDown();

            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                if (m_gameBoard.IsOverLimit(m_activeShape))
                {
                    GameOver();

                }
                else
                {
                    LandShape();
                }
            }
        }
        else if (Input.GetButtonDown("ToggleRot"))
        {
            ToggleRotDirection();
        }
        else if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }
    }

    private void PlaySound(AudioClip clip, float volMultiplier = 1.0f)
    {
        if (clip && m_soundManager.m_fxEnabled)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, Mathf.Clamp(m_soundManager.m_fxVolume * volMultiplier, 0.05f, 1f));
        }
    }

    void LandShape()
    {
        if (m_activeShape)
        {
            // move the shape up, store it in the Board's grid array
            m_activeShape.MoveUp();
            m_gameBoard.StoreShapeInGrid(m_activeShape);

            m_activeShape.LandShapeFX();

            if (m_ghost)
            {
                m_ghost.Reset();
            }

            // spawn a new shape
            m_activeShape = m_spawner.SpawnShape();

            // set all of the timeToNextKey variables to current time, so no input delay for the next spawned shape
            m_timeToNextKeyLeftRight = Time.time;
            m_timeToNextKeyDown = Time.time;
            m_timeToNextKeyRotate = Time.time;

            // remove completed rows from the board if we have any
            m_gameBoard.StartCoroutine("ClearAllRows");


            PlaySound(m_soundManager.m_dropSound, 0.75f);

            if (m_gameBoard.m_completedRows > 0)
            {
                m_scoreManager.ScoreLines(m_gameBoard.m_completedRows);

                if (m_scoreManager.didLevelUp)
                {
                    PlaySound(m_soundManager.m_levelUpVocalClip);
                    m_dropIntervalModded = Mathf.Clamp(m_dropInterval - (((float) m_scoreManager.m_level - 1) * 0.05f), 0.05f, 1);
                }
                else
                {
                    if (m_gameBoard.m_completedRows > 1)
                    {
                        AudioClip randomVocal = m_soundManager.GetRandomClip(m_soundManager.m_vocalClips);
                        PlaySound(randomVocal);
                    }
                }

                PlaySound(m_soundManager.m_clearRowSound);
            }
        }
    }

    private void GameOver()
    {
        // move the shape one row up
        m_activeShape.MoveUp();

        // turn on the Game Over panel
        if (m_gameOverPanel)
        {
            m_gameOverPanel.SetActive(true);
        }
        PlaySound(m_soundManager.m_gameOverSound, 5f);
        PlaySound(m_soundManager.m_gameOverVocalClip, 5f);

        // set the game over condition to true
        m_gameOver = true;
    }


    // reloads the currently active scene
    public static void ReloadLevel()
    {
        Time.timeScale = 1f;
        // reload by scene name
        Debug.Log("Level reloaded");
        LoadLevel(SceneManager.GetActiveScene().name);

        // or by index
        // LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }

    // loads a level by index with some error checking
    public static void LoadLevel(int levelIndex)
    {
        // if the index is valid...
        if (levelIndex >= 0 && levelIndex < SceneManager.sceneCountInBuildSettings)
        {
            // load the scene by index
            SceneManager.LoadScene(levelIndex);
        }
        else
        {
            Debug.LogWarning("LoadLevel Error: invalid scene specified!");
        }
    }

    // loads a level by name with error checking
    public static void LoadLevel(string levelName)
    {
        // if the scene is in the BuildSettings, load the scene
        if (Application.CanStreamedLevelBeLoaded(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogWarning("LoadLevel Error: invalid scene specified!");
        }
    }

    public void ToggleRotDirection()
    {
        m_clockwise = !m_clockwise;
        if (m_rotIconToggle)
        {
            m_rotIconToggle.ToggleIcon(m_clockwise);
        }
    }

    public void TogglePause()
    {
        if (m_gameOver)
        {
            return;
        }

        m_isPaused = !m_isPaused;

        if (m_pausePanel)
        {
            m_pausePanel.SetActive(m_isPaused);

            if (m_soundManager)
            {
                m_soundManager.m_musicSource.volume = (m_isPaused) ? m_soundManager.m_musicVolume * 0.25f : m_soundManager.m_musicVolume;
            }

            Time.timeScale = (m_isPaused) ? 0 : 1;
        }
    }
}
