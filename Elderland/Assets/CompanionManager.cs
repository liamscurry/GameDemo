using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionManager : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particleTrail;

    private Queue<CompanionTrack> Tracks { get; set; } 
    private Queue<bool> MatchSpeeds { get; set; }

    private bool following;
    private int index;
    private CompanionTrack track;
    private bool matchSpeed;
    private float endWaitTimer;

    private void Awake()
    {
        Tracks = new Queue<CompanionTrack>();
        MatchSpeeds = new Queue<bool>();
    }

    public void FollowTrack()
    {
        if (!following && Tracks.Count > 0)
        {
            following = true;
            index = 0;
            track = Tracks.Dequeue();
            matchSpeed = MatchSpeeds.Dequeue();
            track.TryCalculate();
            StartCoroutine(FollowTrackCoroutine());
        }
    }

    public void AddTrack(CompanionTrack track)
    {
        Tracks.Enqueue(track);
        MatchSpeeds.Enqueue(track.MatchSpeed);
    }

    public void Teleport(Transform otherTransform)
    {
        particleTrail.Clear();
        particleTrail.Pause();
        transform.position = otherTransform.position;
        transform.rotation = otherTransform.rotation;
        particleTrail.Play();
    }

    private IEnumerator FollowTrackCoroutine()
    {
        while (true)
        {
            float sprintModifier = 1;
            if (matchSpeed)
            {
                float playerTargetSpeed = (PlayerInfo.MovementManager.CurrentPercentileSpeed);
                sprintModifier = (playerTargetSpeed > 1) ? playerTargetSpeed : 1;
            }

            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = track.Waypoints[index].position;
            Vector3 incrementedPosition = Vector3.MoveTowards(currentPosition, targetPosition, track.Waypoints[index].speed * sprintModifier * Time.deltaTime);
            transform.position = incrementedPosition;

            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = track.Waypoints[index].rotation;
            Quaternion incrementedRotation = Quaternion.RotateTowards(currentRotation, targetRotation, track.Waypoints[index].rotationSpeed * sprintModifier * Time.deltaTime);
            transform.rotation = incrementedRotation;

            if (Vector3.Distance(currentPosition, targetPosition) < 0.05f)
            {
                endWaitTimer = 0;
                if (track.Waypoints[index].endWaitTime > 0)
                {
                    while (true)
                    {
                        sprintModifier = 1;
                        if (matchSpeed)
                        {
                            float playerTargetSpeed = (PlayerInfo.MovementManager.CurrentPercentileSpeed);
                            sprintModifier = (playerTargetSpeed > 1) ? playerTargetSpeed : 1;
                        }

                        endWaitTimer += Time.deltaTime * sprintModifier;
                        Quaternion currentEndRotation = transform.rotation;
                        Quaternion targetEndRotation = track.Waypoints[index].rotation;
                        Quaternion incrementedEndRotation = Quaternion.RotateTowards(currentEndRotation, targetEndRotation, track.Waypoints[index].rotationSpeed * sprintModifier * Time.deltaTime);
                        transform.rotation = incrementedEndRotation;
                        if (endWaitTimer >= track.Waypoints[index].endWaitTime)
                        {
                            break;
                        }
                        else
                        {
                            yield return new WaitForEndOfFrame();
                        }
                    }
                }

                index++;

                if (index == track.Waypoints.Length)
                {
                    track.EndEvents.Invoke();
            
                    if (Tracks.Count > 0)
                    {
                        index = 0;
                        track = Tracks.Dequeue();
                        matchSpeed = MatchSpeeds.Dequeue();
                        track.TryCalculate();
                    }
                    else
                    {
                        following = false;
                        break;
                    }
                }
            }
            
            yield return new WaitForEndOfFrame();
        }
    }
}