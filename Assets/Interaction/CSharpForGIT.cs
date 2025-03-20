using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
// using Unity.Tutorials.Core.Editor;

namespace Interaction
{
    public class CSharpForGIT : MonoBehaviour
    {
        private Thread _mThread;
        public string connectionIP = "127.0.0.1";
        public int connectionPort = 25001;
        public GameObject rController;
        public GameObject lController;
        public GameObject rHandPos;
        public GameObject lHandPos;
        public GameObject rHandExtendedPos;
        public float up = 0.3f;
        private Vector3 offsetVectorForBallMoving = new Vector3(0, 0.1f, 0);
        public float step = 0.002f;
        public ScriptAccessor scriptAccessor;

        private ProbabilityDisplayerController _probabilityDisplayerController;

        public bool listenerActive = true;

        private int retries = 0;
        private Vector3 _basePosR;
        private Vector3 _basePosL;

        private Vector3 _targetPosR;
        private Vector3 _targetPosL;

        private IPAddress _localAdd;
        private TcpListener _listener;
        private TcpClient _client;
        private GameControl _receivedData = new();

        private bool _running;
        private bool _incrementPort = false;

        public int mode;

        private bool _animationTriggered = false;
        private int _animationStep = 0;
        private int _animationIteration = 0;
        private GameObject _boxWithBalls;
        private GameObject _emptyBox;
        private GameObject _currentlyUsedBall;
        private Vector3 _baseCurrentyUsedBallPos;
        private List<GameObject> _ballsArray = new List<GameObject>();
        private bool _changeState;

        private Animator handAnimator;
        public GameObject handObject;

        private void Update()
        {
            if (scriptAccessor.controllerMode.controllerMode > 0 || !scriptAccessor.controllerDevice.DevicesDetected)
            {
                rController.transform.position = Vector3.MoveTowards(rController.transform.position, _targetPosR + (mode == 2 ? offsetVectorForBallMoving : new Vector3(0, 0, 0)), step);
                lController.transform.position = Vector3.MoveTowards(lController.transform.position, _targetPosL + (mode == 2 ? offsetVectorForBallMoving : new Vector3(0, 0, 0)), step);
            }

            if (_currentlyUsedBall != null)
            {
                _currentlyUsedBall.transform.position = Vector3.MoveTowards(
                    rController.transform.position + new Vector3(-0.025f, -0.05f, -0.05f),
                    _targetPosR + new Vector3(0, 0, 0),
                    step
                );
            }

            if (_animationTriggered)
            {
                IncrementMovingBallAnimation();
            }

            if (scriptAccessor.gameState is null)
            {
                var objs = FindObjectsOfType<GameState>();
                if (objs.Any()) scriptAccessor.gameState = objs.First();
            }
            if (_incrementPort)
            {
                connectionPort++;
                _incrementPort = false;
                scriptAccessor.controllerDevice.SetIpText();
            }

            if (_changeState)
            {
                _changeState = false;
                ChangeState();
            }
        }

        private void Start()
        {
            //GetLocalIPAddress();
            // handAnimator = handObject.GetComponent<Animator>();

            ThreadStart ts = new ThreadStart(GetInfo);
            _mThread = new Thread(ts);
            _mThread.Start();

            _basePosR = rHandPos.transform.position;
            _basePosL = lHandPos.transform.position;
            _targetPosR = _basePosR;
            _targetPosL = _basePosL;
            print(_basePosL.ToString());

            _probabilityDisplayerController = GameObject.Find("ProbabilityDisplayer").GetComponent<ProbabilityDisplayerController>();
        }

        private void Stop()
        {
            _running = false;
        }



        void GetInfo()
        {
            while (listenerActive)
            {
                try
                {
                    _localAdd = IPAddress.Parse(connectionIP);
                    _listener = new TcpListener(IPAddress.Any, connectionPort);
                    _listener.Start();

                    _client = _listener.AcceptTcpClient();
                    _client.ReceiveTimeout = 20000;
                    _running = true;

                    Debug.Log("Started listening");
                    while (_running)
                    {
                        SendAndReceiveData();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);

                    //Do not add to retries when disconnecting
                    if (e is not PingException) retries++;
                    Thread.Sleep(TimeSpan.FromMilliseconds(2000));
                    if (retries > 5)
                    {
                        connectionPort++;
                        retries = 0;
                        _incrementPort = true;
                        Debug.Log("Changing port to :" + connectionPort);
                    }
                }
                finally
                {
                    _listener.Stop();
                    Debug.Log("STOPPED");
                }
            }
        }

        void SendAndReceiveData()
        {
            var nwStream = _client.GetStream();
            var buffer = new byte[_client.ReceiveBufferSize];

            //---receiving Data from the Host----
            var bytesRead = nwStream.Read(buffer, 0, _client.ReceiveBufferSize); //Getting data in Bytes from Python
            var dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string

            if (String.IsNullOrWhiteSpace(dataReceived)) throw new PingException("TCP Socket disconnected");
            //---Using received data---
            UpdateReceivedData(JsonUtility.FromJson<GameControl>(dataReceived)); //<-- assigning gameControl value from Python

            UseReceivedData().ConfigureAwait(false);

            //---Getting the game state from the scene---
            var jsonString = JsonUtility.ToJson(scriptAccessor.gameState.Value, true);

            //Debug.Log(jsonString);
            //---Sending Data to Host----
            var myWriteBuffer = Encoding.ASCII.GetBytes(jsonString); //Converting string to byte data
            nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python
        }

        private void UpdateReceivedData(GameControl fromJson)
        {
            Debug.Log(JsonUtility.ToJson(fromJson, true));
            _receivedData.movement = fromJson.movement;
            _receivedData.left = fromJson.left;
            _receivedData.right = fromJson.right;
            _receivedData.leftProbability = fromJson.leftProbability;
            _receivedData.rightProbability = fromJson.rightProbability;
            // scriptAccessor.controllerMode.SetIsAnimationRunning(_receivedData.MoveAnyLimb); // przekazanie czy animacja powinna teraz być odgrywana
            if (fromJson.applyMode && fromJson.mode != _receivedData.mode)
            {
                _receivedData.mode = fromJson.mode;
                scriptAccessor.controllerMode.ChangeMode(fromJson.mode);
            }

            if (fromJson.dataAcquisition != _receivedData.dataAcquisition)
            {
                _changeState = true;
                _receivedData.dataAcquisition = fromJson.dataAcquisition;
            }
        }

        private void ChangeState()
        {
            Debug.Log($"Changed dataAcquisition state to {_receivedData.dataAcquisition}");
            if ((_receivedData.dataAcquisition && _receivedData.mode > 2) || (_receivedData.dataAcquisition && _receivedData.mode <= 2))
                scriptAccessor.controllerMode.SetUpGripping();
            else
                scriptAccessor.controllerMode.SetUpHandsForBallz();

            var x = FindObjectOfType<ButtonChangeGameState>();
            x.Increment();
            x.SwitchColorAndState();
        }

        public async Task UseReceivedData()
        {
            await Task.Run(() =>
            {
                switch (mode)
                {
                    case 0:
                        //Controller mode - do nothing
                        break;
                    case 1:
                        AnimateLift();
                        break;
                    case 2:
                        //TODO: ANIMATION
                        AnimateBallsMoving();
                        break;
                    case 3:
                        //TODO: GRIP
                        AnimateGrip();
                        break;
                    case 4:
                        //TODO: GRIP
                        AnimateGrip();
                        break;
                    case 5: case 6: case 7: case 8: case 9: case 10: case 11:
                        PlayHandExercise();
                        break;
                    default:
                        break;
                }
            });
        }

        private void AnimateLift()
        {
            _targetPosR = _basePosR;
            _targetPosL = _basePosL;

            if (!_receivedData.MoveAnyLimb) return;
            _targetPosR += new Vector3(0, 1 * up, 0);
            _targetPosL += new Vector3(0, 1 * up, 0);
        }

        private void AnimateGrip()
        {
            _targetPosR = _basePosR;
            _targetPosL = _basePosL;

            scriptAccessor.TriggerAnimation(_receivedData.MoveAnyLimb);
        }

        private void AnimateBallsMoving()
        {
            _targetPosR = _basePosR;
            _targetPosL = _basePosL;

            if (_receivedData.MoveAnyLimb)
            {
                _animationTriggered = true;
            }
        }

        private void PlayHandExercise()
        {
            scriptAccessor.TriggerAnimation(_receivedData.MoveAnyLimb);
            DisplayProbability(_receivedData.leftProbability);
        }

        private void DisplayProbability(float probability)
        {
            _probabilityDisplayerController.ChangeColor(probability);
        }
        // private void ExerciseOne()
        // {
        //     if (_receivedData.MoveAnyLimb)
        //     {
        //         handAnimator.SetBool("Is_Stop", false);
        //     handAnimator.SetBool("Is_Exercise_1", true);
        //     handAnimator.SetTrigger("Exercise_1");
        //     }
        //     else
        //     {
        //         handAnimator.SetBool("Is_Exercise_1", false);
        //         // handAnimator.SetBool("Is_Stop", true);
        //     }
        // }
        //
        // private void ExerciseTwoA()
        // {
        //     if (_receivedData.MoveAnyLimb)
        //     {
        //         handAnimator.SetBool("Is_Stop", false);
        //         handAnimator.SetBool("Is_Exercise_2A", true);
        //     }
        //     else
        //     {
        //         handAnimator.SetBool("Is_Exercise_2A", false);
        //         handAnimator.SetBool("Is_Stop", true);
        //     }
        // }
        //
        // private void ExerciseSeven()
        // {
        //     if (_receivedData.MoveAnyLimb)
        //     {
        //         handAnimator.SetBool("Is_Stop", false);
        //         handAnimator.SetBool("Is_Exercise_7", true);
        //     }
        //     else
        //     {
        //         handAnimator.SetBool("Is_Exercise_7", false);
        //         handAnimator.SetBool("Is_Stop", true);
        //     }
        // }

        private static Vector3 StringToVector3(string sVector)
        {
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            {
                sVector = sVector.Substring(1, sVector.Length - 2);
            }

            // split the items
            var sArray = sVector.Split(',');

            // store as a Vector3
            var result = new Vector3(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));

            return result;
        }

        private void SetBoxVariables()
        {
            if (_boxWithBalls == null || (_ballsArray.Count > 0 && _ballsArray[0] == null))
            {
                _boxWithBalls = GameObject.Find("BoxWithBalls(Clone)");

                if (_boxWithBalls == null)
                {
                    return;
                }

                _emptyBox = GameObject.Find("EmptyBox(Clone)");
                _ballsArray = new List<GameObject>();

                foreach (Transform child in _boxWithBalls.transform)
                {
                    if (child.tag == "Ball")
                    {
                        _ballsArray.Add(child.gameObject);
                    }
                }
            }
        }

        private void nextStepAnimationStep()
        {
            if ((_targetPosR + offsetVectorForBallMoving) == rController.transform.position)
            {
                _animationStep++;

                if (_animationStep == 12)
                {
                    _animationStep = 0;
                    _animationTriggered = false;
                    _animationIteration++;

                    if (_animationIteration >= _ballsArray.Count) {
                        scriptAccessor.controllerMode.SetUpMode();
                        _animationIteration = 0;
                    }
                }
            }
        }

        private void IncrementMovingBallAnimation()
        {
            SetBoxVariables();

            if (_boxWithBalls == null)
            {
                return;
            }

            switch (_animationStep)
            {
                case 0:
                    // Reka byla na pozycji startowej wiec teraz nalezy przeniesc reke nad pilke
                    if (_currentlyUsedBall == null)
                    {
                        _baseCurrentyUsedBallPos = _ballsArray[_animationIteration].transform.position;
                    }

                    _targetPosR = _baseCurrentyUsedBallPos;
                    nextStepAnimationStep();
                    break;
                case 1:
                    // Opusc reke do pilki
                    _targetPosR = _baseCurrentyUsedBallPos + new Vector3(0, -0.05f, 0);
                    nextStepAnimationStep();
                    break;
                case 2:
                    // Reka znajduje sie na pozycji odpowiedniej pilki wiec trzeba zlapac
                    if (_currentlyUsedBall == null)
                    {
                        _currentlyUsedBall = _ballsArray[_animationIteration];
                        scriptAccessor.TriggerAnimation(true);
                    }

                    nextStepAnimationStep();
                    break;
                case 3:
                    // Reka trzyma pilke trzeba wrocic do pozycji startowej
                    _targetPosR = _basePosR;
                    nextStepAnimationStep();
                    break;
                case 4:
                    // Wysunięcie ręki żeby rych był bardziej naturalny
                    _targetPosR = rHandExtendedPos.transform.position + new Vector3(0.12f, 0, 0);
                    nextStepAnimationStep();
                    break;
                case 5:
                    // Przesunięcie ręki w lewo
                    _targetPosR = rHandExtendedPos.transform.position;
                    nextStepAnimationStep();
                    break;
                case 6:
                    // Reka w pozycji startowej ale z pilka, przenosimy do pozycji bazowej lewej reki
                    _targetPosR = _basePosL + new Vector3(0.14f, 0, 0);
                    nextStepAnimationStep();
                    break;
                case 7:
                    // Reka w pozycji lewej reki startowej, trzeba nakierowac na puste pudlo
                    _targetPosR = _emptyBox.transform.position + new Vector3(0.04f, 0, 0);
                    nextStepAnimationStep();
                    break;
                case 8:
                    // Reka z pilka w pustym pudle
                    nextStepAnimationStep();
                    break;
                case 9:
                    // Dodatkowa przerwa zeby napewno gostek puscil w dobrym miejscu
                    if (_currentlyUsedBall != null) {
                        _currentlyUsedBall = null;
                    }
                    scriptAccessor.TriggerAnimation(false);
                    nextStepAnimationStep();
                    break;
                case 10:
                    // wyciagamy reke na pozycje startowa lewej reki
                    _targetPosR = _basePosL + new Vector3(0.14f, 0, 0);
                    nextStepAnimationStep();
                    break;
                case 11:
                    // wracamy na pozycje prawej reki i konczymy petle
                    _targetPosR = _basePosR;
                    nextStepAnimationStep();
                    // IncrementAnimationIteration();
                    break;
            }
        }
    }
}