using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBehaviour : StateMachineBehaviour 
{
    private bool exiting;
    private float movespeedVelocity;

    private bool sprinting;
    private const float sprintDisableAngle = 50;
    private float currentReverse;
    private Vector2 positionAnalogDirection;
    private Vector2 reverseAnalogDirection;

    private const float positionAnalogSpeed = 1.7f;
    private const float reverseAnalogSpeed = 1.35f;

    private const float reverseSpeed = 42.5f;
    private const float reverseSpeedSlow = 0.0f;
    private const float reverseZeroMin = 0.05f;
    private const float reverseMargin = 0.05f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		exiting = false;
        movespeedVelocity = 0;
        sprinting = false;
        PlayerInfo.StatsManager.Sprinting = false;
        currentReverse = 0;
        positionAnalogDirection = Vector2.zero;
        PlayerInfo.MovementManager.PercentileSpeed = 
            PlayerInfo.MovementManager.CurrentPercentileSpeed;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{   
        if (!exiting && GameInfo.Manager.ReceivingInput)
        {
            Vector2 projectedCameraDirection = Matho.StandardProjection2D(GameInfo.CameraController.Direction).normalized;
            Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
            Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
            Vector2 movementDirection = forwardDirection + sidewaysDirection;

            UpdateSprinting(GameInfo.Settings.LeftDirectionalInput);

            //Direction and speed targets
            if (GameInfo.Settings.LeftDirectionalInput.magnitude <= 0.5f)
            {
                PlayerInfo.MovementManager.LockDirection();
                PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
                sprinting = false;
                PlayerInfo.StatsManager.Sprinting = false;
            }
            else
            {
                Vector3 targetRotation = Matho.StandardProjection3D(GameInfo.CameraController.Direction).normalized;
                Vector3 currentRotation = Matho.StandardProjection3D(PlayerInfo.Player.transform.forward).normalized;
                Vector3 incrementedRotation = Vector3.RotateTowards(currentRotation, targetRotation, 10 * Time.deltaTime, 0f);
                Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
                PlayerInfo.Player.transform.rotation = rotation;

                PlayerInfo.MovementManager.TargetDirection = movementDirection;

                float forwardsAngle = Matho.AngleBetween(Matho.StandardProjection2D(targetRotation), movementDirection);
                float forwardsModifier = Mathf.Cos(forwardsAngle * 0.4f * Mathf.Deg2Rad);
                
                float sprintingModifier = (sprinting) ? 2f : 1f;
           
                PlayerInfo.MovementManager.TargetPercentileSpeed =
                    GameInfo.Settings.LeftDirectionalInput.magnitude * forwardsModifier * sprintingModifier;
            }

            PlayerInfo.MovementSystem.Move(
                PlayerInfo.MovementManager.CurrentDirection,
                PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.Movespeed);

            UpdateAnimatorProperties(
                animator,
                projectedCameraDirection,
                Matho.Rotate(projectedCameraDirection, 90));

            //Transitions//
            if (!animator.IsInTransition(0) && PlayerInfo.AbilityManager.CurrentAbility == null)
            {
                //FallingTransition(animator, stateInfo, layerIndex);
                LadderTransition(animator, stateInfo, layerIndex);
                if (!exiting)
                    MantleTransition(animator, stateInfo, layerIndex);
                //if (!exiting)
                //    JumpTransition(animator, stateInfo, layerIndex);
            }
        }
	}

    private void UpdateAnimatorProperties(
        Animator animator,
        Vector2 forwardDir,
        Vector2 rightDir)
    {
        if (forwardDir.magnitude == 0 ||
            rightDir.magnitude == 0 || 
            PlayerInfo.MovementManager.TargetDirection.magnitude == 0 ||
            PlayerInfo.MovementManager.CurrentDirection.magnitude == 0)
            return;

        float speed = 
            PlayerInfo.MovementManager.CurrentPercentileSpeed *
            PlayerInfo.StatsManager.MovespeedMultiplier.Value;

        Vector2 scaledCurrentDir = 
            PlayerInfo.MovementManager.CurrentDirection * PlayerInfo.MovementManager.CurrentPercentileSpeed;
        Vector2 analogDirection = 
            new Vector2(
                Matho.ProjectScalar(scaledCurrentDir, forwardDir),
                Matho.ProjectScalar(scaledCurrentDir, rightDir));

        Vector2 rAnalogDirection = reverseAnalogDirection;

        if (analogDirection.x < 0)
            analogDirection.x *= 3;
        Vector2 effectiveAnalogDir = reverseAnalogDirection;
        if (effectiveAnalogDir.x < 0)
            effectiveAnalogDir.x *= 3;

        if (analogDirection.x > 0 &&
            Matho.AngleBetween(Vector2.up, new Vector2(analogDirection.y, analogDirection.x)) > 60f)
        {
            analogDirection.x = 0;
        }
        else if (
            analogDirection.x < 0 &&
            Matho.AngleBetween(Vector2.down, new Vector2(analogDirection.y, analogDirection.x)) > 60f)
        {
            analogDirection.x = 0;
        }

        positionAnalogDirection =
            Vector2.MoveTowards(positionAnalogDirection, analogDirection, positionAnalogSpeed * Time.deltaTime);
        reverseAnalogDirection =
            Vector2.MoveTowards(reverseAnalogDirection, analogDirection, reverseAnalogSpeed * Time.deltaTime);


        animator.SetFloat(
            "speed",
            positionAnalogDirection.x);
        animator.SetFloat(
            "strafe",
            positionAnalogDirection.y);
        animator.SetFloat(
            "percentileSpeed",
            PlayerInfo.MovementManager.PercentileSpeed);
        
        /*
        CalculateReverseParameters(
            animator,
            forwardDir,
            rightDir,
            analogDirection);
            */
        //animator.SetFloat("reverse", currentReverse);
    }

    private void CalculateReverseParameters(
        Animator animator,
        Vector2 forwardDir,
        Vector2 rightDir,
        Vector2 analogDirection)
    {
        Vector2 reverseOffset = analogDirection - reverseAnalogDirection;
        if (reverseOffset.magnitude > 1)
            reverseOffset.Normalize();
            
        if (reverseOffset.magnitude < reverseMargin)
        {
            animator.SetFloat(
                "speedReverse",
                0);
            animator.SetFloat(
                "strafeReverse",
                0);
            return;
        }
        
        float reverseLimiter = Matho.AngleBetween(reverseAnalogDirection, reverseOffset) / 180f;

        reverseLimiter -= 0.5f;
        if (reverseLimiter < 0)
            reverseLimiter = 0;
        reverseLimiter *= 2f;
        reverseLimiter *= reverseAnalogDirection.magnitude;

        animator.SetFloat(
            "speedReverse",
            reverseOffset.x * reverseLimiter);
        animator.SetFloat(
            "strafeReverse",
            reverseOffset.y * reverseLimiter);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        sprinting = false;
        PlayerInfo.StatsManager.Sprinting = false;
	}

    private void UpdateSprinting(Vector2 analogMovementDirection)
    {
        if (Matho.AngleBetween(Vector2.up, analogMovementDirection) > sprintDisableAngle)
        {
            sprinting = false;
            PlayerInfo.StatsManager.Sprinting = false;
            return;
        }

        if (Input.GetKeyDown(GameInfo.Settings.SprintKey) && PlayerInfo.MovementManager.SprintAvailable)
        {
            sprinting = true;
            PlayerInfo.StatsManager.Sprinting = true;
        }
    }

    private void FallingTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayerInfo.PhysicsSystem.ExitedFloor)
        {
            animator.SetBool("falling", true);
            exiting = true;
        }
    }

    private void JumpTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Input.GetKeyDown(GameInfo.Settings.JumpKey) && PlayerInfo.PhysicsSystem.LastCalculatedVelocity.magnitude < PlayerInfo.StatsManager.BaseMovespeed * 2.5f)
        {
            animator.SetBool("jump", true);
            PlayerInfo.MovementManager.LockDirection();
            PlayerInfo.MovementManager.TargetPercentileSpeed = 1;
            exiting = true;
        }
    }

	private void LadderTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Vector2 projectedCameraDirection = Matho.StandardProjection2D(GameInfo.CameraController.Direction).normalized;
        Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
        Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
        Vector2 movementDirection = forwardDirection + sidewaysDirection;

        if (PlayerInfo.Sensor.LadderBottom != null)
        {
            //Potential ladder to attach to
            Ladder contactLadder = PlayerInfo.Sensor.LadderBottom.transform.parent.GetComponent<Ladder>();

            //Normal distance
            float normalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, contactLadder.Normal);

            //Horizontal positioning
            float horizontalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, contactLadder.RightDirection);

            //Direction
            Vector2 invertedNormal = Matho.Rotate(Matho.StandardProjection2D(contactLadder.Normal), 180);

            if (normalProjectionScalar > 0 &&
                Mathf.Abs(horizontalProjectionScalar) < contactLadder.Width / 2 - PlayerInfo.Capsule.radius &&
                Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius < (contactLadder.Depth / 2) + 1f &&
                Matho.AngleBetween(invertedNormal, movementDirection) < 45)
            {
                //Specific
                Ladder ladder = PlayerInfo.Sensor.LadderBottom.transform.parent.GetComponent<Ladder>();
                PlayerInfo.InteractionManager.Ladder = ladder;

                //Generate target positions
                Vector3 walkTargetPosition = ladder.transform.position;
                walkTargetPosition += ladder.Normal * (PlayerInfo.Capsule.radius + ladder.Depth / 2);
                walkTargetPosition += ladder.RightDirection * horizontalProjectionScalar;
                walkTargetPosition.y = ladder.transform.position.y - (ladder.Height / 2) + (PlayerInfo.Capsule.height / 2);

                Vector3 climbTargetPosition = walkTargetPosition + Vector3.up * 0.5f;

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, -ladder.Normal);

                var walkTarget = new PlayerAnimationManager.MatchTarget(walkTargetPosition, targetRotation, AvatarTarget.Root, Vector3.one, 1);
                var climbTarget = new PlayerAnimationManager.MatchTarget(climbTargetPosition, targetRotation, AvatarTarget.Root, Vector3.one, 1); 
                PlayerInfo.AnimationManager.EnqueueTarget(walkTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(climbTarget);

                animator.SetTrigger("interacting");
                animator.SetTrigger("climbing");
                animator.SetTrigger("climbEnterBottom");

                exiting = true;
                GameInfo.Manager.FreezeInput(this);
                PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
                GameInfo.CameraController.AllowZoom = false;
                PlayerInfo.AnimationManager.Interuptable = false;
            }
        }

        if (PlayerInfo.Sensor.LadderTop != null)
        {
            //Potential ladder to attach to
            Ladder contactLadder = PlayerInfo.Sensor.LadderTop.transform.parent.GetComponent<Ladder>();

            //Normal distance
            //float normalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, contactLadder.Normal);
            float normalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, -contactLadder.Normal);

            //Horizontal positioning
            float horizontalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, contactLadder.RightDirection);

            //Direction
            Vector2 normal = Matho.StandardProjection2D(contactLadder.Normal);

            if (normalProjectionScalar > 0 &&
                Mathf.Abs(horizontalProjectionScalar) < contactLadder.Width / 2 - PlayerInfo.Capsule.radius &&
                Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius < (contactLadder.Depth / 2) + 1f &&
                Matho.AngleBetween(normal, movementDirection) < 45)
            {
                //Specific
                Ladder ladder = PlayerInfo.Sensor.LadderTop.transform.parent.GetComponent<Ladder>();
                PlayerInfo.InteractionManager.Ladder = ladder;
                
                //Generate target positions
                Vector3 walkTargetPosition = ladder.transform.position;
                walkTargetPosition += ladder.Normal * (PlayerInfo.Capsule.radius + ladder.Depth / 2);
                walkTargetPosition += ladder.RightDirection * horizontalProjectionScalar;
                walkTargetPosition.y = ladder.transform.position.y + (ladder.Height / 2) + (PlayerInfo.Capsule.height / 2);

                Quaternion walkTargetRotation = Quaternion.FromToRotation(Vector3.forward, ladder.Normal);

                Vector3 climbTargetPosition = ladder.transform.position;
                climbTargetPosition += ladder.Normal * (PlayerInfo.Capsule.radius + ladder.Depth / 2);
                climbTargetPosition += ladder.RightDirection * horizontalProjectionScalar;
                climbTargetPosition.y = ladder.transform.position.y + (ladder.Height / 2);

                Quaternion climbTargetRotation = Quaternion.FromToRotation(Vector3.forward, -ladder.Normal);

                var walkTarget = new PlayerAnimationManager.MatchTarget(walkTargetPosition, walkTargetRotation, AvatarTarget.Root, Vector3.one, 1);
                var climbTarget = new PlayerAnimationManager.MatchTarget(climbTargetPosition, climbTargetRotation, AvatarTarget.Root, Vector3.one, 1); 
                PlayerInfo.AnimationManager.EnqueueTarget(walkTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(climbTarget);

                animator.SetTrigger("interacting");
                animator.SetTrigger("climbing");
                animator.SetTrigger("climbEnterTop");

                exiting = true;
                GameInfo.Manager.FreezeInput(this);
                PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
                GameInfo.CameraController.AllowZoom = false;
                PlayerInfo.AnimationManager.Interuptable = false;
            }
        }
	}

    private void MantleTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayerInfo.Sensor.MantleTop != null)
        {
            MantleDown(animator, stateInfo, layerIndex);
        }

        if (PlayerInfo.Sensor.MantleBottom != null)
        {
            MantleUp(animator, stateInfo, layerIndex);
        }
    }

    private void MantleDown(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector2 projectedCameraDirection = Matho.StandardProjection2D(GameInfo.CameraController.Direction).normalized;
        Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
        Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
        Vector2 movementDirection = Matho.StandardProjection2D(PlayerInfo.Player.transform.forward);

        //Potential mantle to attach to
        Mantle mantle = PlayerInfo.Sensor.MantleTop;

        //Normal distance
        float normalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - mantle.transform.position, -mantle.Normal);

        if (normalProjectionScalar > 0 &&
            Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius < 1f &&
            Matho.AngleBetween(Matho.StandardProjection2D(mantle.Normal), movementDirection) < 45)
        {
            //generate target positions
            Vector3 ledgePosition = PlayerInfo.Player.transform.position;
            ledgePosition += (Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius) * mantle.Normal;
            ledgePosition.y = mantle.TopVerticalPosition + PlayerInfo.Capsule.height / 2;

            Vector3 basePosition = ledgePosition;
            basePosition += (PlayerInfo.Capsule.radius * 2) * mantle.Normal;
            basePosition.y = mantle.BottomVerticalPosition + PlayerInfo.Capsule.height / 2;

            Vector3 walkPosition = basePosition + 1f * mantle.Normal;

            Quaternion inverseNormalRotation = Quaternion.LookRotation(-mantle.Normal, Vector3.up);
            Quaternion normalRotation = Quaternion.LookRotation(mantle.Normal, Vector3.up);

            //create and queue match targets
            if (mantle.Type == Mantle.MantleType.Short)
            {
                var ledgeTarget = new PlayerAnimationManager.MatchTarget(ledgePosition, normalRotation, AvatarTarget.Root, Vector3.one * 1.5f, 1); 
                var landTarget = new PlayerAnimationManager.MatchTarget(basePosition, Quaternion.identity, AvatarTarget.Root, Vector3.one, 0);
                PlayerInfo.AnimationManager.EnqueueTarget(ledgeTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(landTarget);
            }
            else
            {
                var ledgeTarget = new PlayerAnimationManager.MatchTarget(ledgePosition, inverseNormalRotation, AvatarTarget.Root, Vector3.one * 1.5f, 1); 
                var landTarget = new PlayerAnimationManager.MatchTarget(basePosition, Quaternion.identity, AvatarTarget.Root, Vector3.one, 0);
                var standupTarget = new PlayerAnimationManager.MatchTarget(basePosition, normalRotation, AvatarTarget.Root, Vector3.zero, 1);
                PlayerInfo.AnimationManager.EnqueueTarget(ledgeTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(landTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(standupTarget);
            }
            
            
            PlayerInfo.Manager.test = ledgePosition;
            PlayerInfo.Manager.test2 = basePosition;

            //transition animator
            animator.SetTrigger("interacting");
            animator.SetTrigger("mantle");
            
            if (mantle.Type == Mantle.MantleType.Short)
                animator.SetTrigger("shortMantle");
            else
                animator.SetTrigger("tallMantle");

            animator.SetTrigger("mantleTop");
            GameInfo.Manager.FreezeInput(this);
            PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
            GameInfo.CameraController.AllowZoom = false;
            PlayerInfo.AnimationManager.Interuptable = false;
            GameInfo.CameraController.TargetDirection = mantle.Normal;
            exiting = true;
        }
    }

    private void MantleUp(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector2 projectedCameraDirection = Matho.StandardProjection2D(GameInfo.CameraController.Direction).normalized;
        Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
        Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
        Vector2 facingDirection = Matho.StandardProjection2D(PlayerInfo.Player.transform.forward);

        //Potential mantle to attach to
        Mantle mantle = PlayerInfo.Sensor.MantleBottom;

        //Normal distance
        float normalProjectionScalar = Mathf.Abs(Matho.ProjectScalar(PlayerInfo.Player.transform.position - mantle.transform.position, mantle.Normal));

        if (normalProjectionScalar > 0 &&
            Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius < 1f &&
            Matho.AngleBetween(Matho.StandardProjection2D(-mantle.Normal), facingDirection) < 45 &&
            (mantle.Type == Mantle.MantleType.Short || (mantle.Type == Mantle.MantleType.Tall && Input.GetKeyDown(GameInfo.Settings.JumpKey))))
        {
            //generate target positions
            Vector3 basePosition = PlayerInfo.Player.transform.position;
            basePosition += (Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius) * -mantle.Normal;
            basePosition.y = mantle.BottomVerticalPosition + PlayerInfo.Capsule.height / 2;

            Vector3 ledgePosition = basePosition;
            ledgePosition.y = (mantle.Type == Mantle.MantleType.Short) ? mantle.TopVerticalPosition : mantle.TopVerticalPosition - (PlayerInfo.Capsule.height / 2);

            Vector3 walkPosition = new Vector3(0, mantle.TopVerticalPosition + PlayerInfo.Capsule.height / 2, 0);

            Quaternion rotation = Quaternion.FromToRotation(PlayerInfo.Player.transform.forward, -mantle.Normal) * PlayerInfo.Player.transform.rotation;

            //create and queue match targets
            var baseTarget = new PlayerAnimationManager.MatchTarget(basePosition, rotation, AvatarTarget.Root, Vector3.one, 1);
            var ledgeTarget = new PlayerAnimationManager.MatchTarget(ledgePosition, rotation, AvatarTarget.Root, Vector3.one, 0); 
            var walkTarget = new PlayerAnimationManager.MatchTarget(walkPosition, Quaternion.identity, AvatarTarget.Root, new Vector3(0, 1, 0), 0, 0.9f, 1); 
            PlayerInfo.AnimationManager.EnqueueTarget(baseTarget);
            PlayerInfo.AnimationManager.EnqueueTarget(ledgeTarget);
            PlayerInfo.AnimationManager.EnqueueTarget(walkTarget);

            PlayerInfo.Manager.test = basePosition;
            PlayerInfo.Manager.test2 = ledgePosition;

            //transition animator
            animator.SetTrigger("interacting");
            animator.SetTrigger("mantle");
            
            if (mantle.Type == Mantle.MantleType.Short)
                animator.SetTrigger("shortMantle");
            else
                animator.SetTrigger("tallMantle");

            animator.SetTrigger("mantleBottom");
            GameInfo.Manager.FreezeInput(this);
            PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
            GameInfo.CameraController.AllowZoom = false;
            PlayerInfo.AnimationManager.Interuptable = false;
            GameInfo.CameraController.TargetDirection = mantle.Normal;
            exiting = true;
        }
    }
}