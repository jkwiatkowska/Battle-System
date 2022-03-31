using System;
using System.Collections;
using UnityEngine;

public class MovementEntity : MonoBehaviour
{
    [SerializeField] float GroundCheckSphereRadius;
    protected Entity Entity;

    protected Vector3 Velocity;
    public Vector3 EntityVelocity => Velocity;
    public float GravitationalForce                     { get; protected set; }
    public bool IsGrounded                              { get; protected set; }
    Vector3 GroundCheckSphereOffset;

    public bool IsRunning                               { get; protected set; }

    public float LastMoved                              { get; protected set; }
    public virtual void Setup(Entity entity)
    {
        Entity = entity;
        GravitationalForce = Constants.Gravity;
        Velocity = new Vector3();
        GroundCheckSphereOffset = new Vector3(0.0f, GroundCheckSphereRadius - Constants.Epsilon, 0.0f);
    }

    #region Action Movement
    public ActionMovement CurrentMovement               { get; protected set; }
    Coroutine MovementCoroutine;

    public virtual bool InitiateMovement(ActionMovement movement, Entity target = null)
    {
        if (CurrentMovement != null)
        {
            if (CurrentMovement.Priority > movement.Priority)
            {
                return false;
            }
            else
            {
                CancelMovement();
            }
        }

        CurrentMovement = movement;

        switch (movement.MovementType)
        {
            case ActionMovement.eMovementType.MoveToPosition:
            {
                MovementCoroutine = StartCoroutine(MoveToPositionEnumerator(target));
                break;
            }
            case ActionMovement.eMovementType.TeleportToPosition:
            {
                Teleport(movement, target);
                break;
            }
            case ActionMovement.eMovementType.LaunchToPosition:
            {
                MovementCoroutine = StartCoroutine(LaunchToPositionEnumerator(target));
                break;
            }
            case ActionMovement.eMovementType.MoveForward:
            {
                MovementCoroutine = StartCoroutine(MoveAlongForwardEnumerator());
                break;
            }
            case ActionMovement.eMovementType.MoveBackward:
            {
                MovementCoroutine = StartCoroutine(MoveAlongForwardEnumerator(true));
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented movement type: {movement.MovementType}");
                return false;
            }
        }

        return true;
    }

    public virtual void CancelMovement()
    {
        CurrentMovement = null;
        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void Teleport(ActionMovement movement, Entity target)
    {
        // CurrentMovement has to be set first.
        if (movement == null)
        {
            Debug.LogError($"CurrentMovement was not set.");
            return;
        }

        // Find position from transform data. 
        var found = movement.TargetPosition.TryGetTransformFromData(Entity, target, out var goal, out _);
        if (!found)
        {
            Debug.LogError($"A position for movement action used by {Entity.EntityUID} could not be found.");
            movement = null;
            return;
        }

        SetPosition(goal);
    }

    public virtual IEnumerator MoveToPositionEnumerator(Entity target)
    {
        // CurrentMovement has to be set first.
        if (CurrentMovement == null)
        {
            Debug.LogError($"CurrentMovement was not set.");
            yield break;
        }

        // Find position from transform data. 
        var found = CurrentMovement.TargetPosition.TryGetTransformFromData(Entity, target, out var goal, out _);
        if (!found)
        {
            Debug.LogError($"A position for movement action used by {Entity.EntityUID} could not be found.");
            CurrentMovement = null;
            yield break;
        }

        // Rotate toward target position if required.
        var startTime = BattleSystem.Time;
        float totalElapsedTime;
        float elapsedTime;
        float lastTime = startTime;

        Vector3 dir;
        float dist;

        var speed = CurrentMovement.Speed > Constants.Epsilon ? CurrentMovement.Speed :
                    Formulae.EntityMovementSpeed(Entity, false);

        // Move toward position.
        do
        {
            totalElapsedTime = BattleSystem.Time - startTime;
            elapsedTime = BattleSystem.Time - lastTime;
            lastTime = BattleSystem.Time;

            dir = goal - Entity.Origin;
            dist = dir.sqrMagnitude;

            var movement = Mathf.Min(Mathf.Pow(speed * elapsedTime, 2), dist);
            if (CurrentMovement.HorizontalMovementOnly)
            {
                dir.y = 0.0f;
            }

            Move(dir.normalized * movement, CurrentMovement.FaceMovementDirection, setMovementTrigger: false);

            yield return null;
        }
        while (dist > Constants.Epsilon && totalElapsedTime < CurrentMovement.MaxDuration);

        CurrentMovement = null;
    }

    public virtual IEnumerator LaunchToPositionEnumerator(Entity target)
    {
        // CurrentMovement has to be set first.
        if (CurrentMovement == null)
        {
            Debug.LogError($"CurrentMovement was not set.");
            yield break;
        }

        // Find position from transform data. 
        var found = CurrentMovement.TargetPosition.TryGetTransformFromData(Entity, target, out var goal, out _);
        if (!found)
        {
            Debug.LogError($"A position for movement action used by {Entity.EntityUID} could not be found.");
            CurrentMovement = null;
            yield break;
        }

        // Rotate toward target position if required.
        var dir = goal - Entity.Origin;
        if (CurrentMovement.FaceMovementDirection)
        {
            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        var startTime = BattleSystem.Time;
        float elapsedTime;
        float lastTime = startTime;

        var gravityMultiplier = CurrentMovement.Speed > Constants.Epsilon ? CurrentMovement.Speed : 1.0f;
        Velocity = LaunchVelocity(goal, CurrentMovement.LaunchAngle, gravityMultiplier);

        // Move toward position.
        do
        {
            elapsedTime = BattleSystem.Time - lastTime;
            lastTime = BattleSystem.Time;

            Velocity.y += GravitationalForce * gravityMultiplier * elapsedTime;
            yield return null;
        }
        while (!(IsGrounded && Velocity.y < Constants.Epsilon));

        Velocity = Vector3.zero;
        CurrentMovement = null;
    }

    public virtual IEnumerator MoveAlongForwardEnumerator(bool reverse = false)
    {
        // CurrentMovement has to be set first.
        if (CurrentMovement == null)
        {
            Debug.LogError($"CurrentMovement was not set.");
            yield break;
        }

        var startTime = BattleSystem.Time;
        float totalElapsedTime;
        float elapsedTime;
        float lastTime = startTime;

        var directionMultiplier = reverse ? -1.0f : 1.0f;
        var speed = (CurrentMovement.Speed > Constants.Epsilon ? CurrentMovement.Speed :
                     Formulae.EntityMovementSpeed(Entity, false)) * directionMultiplier;

        // Move toward position.
        do
        {
            totalElapsedTime = BattleSystem.Time - startTime;
            elapsedTime = BattleSystem.Time - lastTime;
            lastTime = BattleSystem.Time;

            var dir = transform.forward.normalized * directionMultiplier;
            Move(elapsedTime * speed * dir, faceMovementDirection: false, setMovementTrigger: false);

            yield return null;
        }
        while (totalElapsedTime < CurrentMovement.MaxDuration);

        CurrentMovement = null;
    }
    #endregion

    #region Movement
    public virtual void Move(Vector3 movement, bool faceMovementDirection, bool setMovementTrigger)
    {
        transform.position += movement;
        if (faceMovementDirection)
        {
            transform.rotation = Quaternion.LookRotation(movement.normalized, Vector3.up);
        }

        if (setMovementTrigger)
        {
            Entity.OnEntityMoved();
            LastMoved = BattleSystem.Time;
        }
    }

    public virtual Vector3 Move(Vector3 direction, bool faceMovementDirection, float speedMultiplier = 1.0f, bool setMovementTrigger = true, bool ignoreLock = false)
    {
        if (!ignoreLock && Entity.IsMovementLocked)
        {
            return Vector3.zero;
        }

        direction.Normalize();

        var movement = direction * GetMovementAmount(Entity, Time.fixedDeltaTime, speedMultiplier);
        Move(movement, faceMovementDirection, setMovementTrigger);

        return movement;
    }

    public void Jump()
    {
        if (Entity.IsJumpingLocked)
        {
            return;
        }

        if (IsGrounded)
        {
            Velocity.y += Mathf.Sqrt(Formulae.EntityJumpHeight(Entity) * -2.0f * GravitationalForce);
            CancelMovement();

            Entity.OnEntityJumped();
        }
    }

    public static float GetMovementAmount(Entity entity, float timeElapsed, float speedMultiplier = 1.0f)
    {
        return Formulae.EntityMovementSpeed(entity, entity.Movement.IsRunning) * speedMultiplier * timeElapsed;
    }
    #endregion

    #region Rotation
    public void RotateTowardPosition(Vector3 targetPosition)
    {
        var rotationSpeed = Formulae.EntityRotateSpeed(Entity);
        RotateTowardPosition(targetPosition, rotationSpeed);
    }

    public void RotateTowardPosition(Vector3 targetPosition, float rotationSpeed)
    {
        var targetDirection = targetPosition - transform.position;
        targetDirection.y = 0.0f;
        var initialForward = transform.forward;

        var direction = targetDirection;

        if (Vector3.Angle(transform.forward, targetDirection) > rotationSpeed)
        {
            direction = Vector3.RotateTowards(initialForward, targetDirection, Time.fixedDeltaTime * rotationSpeed, 0.0f);
        }

        transform.rotation = Quaternion.LookRotation(direction);
    }

    public IEnumerator RotateTowardDirectionCoroutine(Vector3 targetDirection, float rotationSpeed)
    {
        targetDirection.y = 0.0f;
        var initialForward = transform.forward;

        var startTime = BattleSystem.Time;

        do
        {
            var timeElapsed = BattleSystem.Time - startTime;
            var direction = Vector3.RotateTowards(initialForward, targetDirection, timeElapsed * rotationSpeed, 0.0f);

            FaceDirection(direction);

            yield return null;
        }
        while (Vector3.Angle(transform.forward, targetDirection) > 0.5f);
        FaceDirection(targetDirection);
    }

    public void FaceDirection(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void RotateAroundYAxis(float rotationAmount)
    {
        transform.Rotate(0.0f, rotationAmount, 0.0f);
    }

    public void RotateAroundYAxis(float rotationPerSecond, ref float targetRotation)
    {
        var rotation = GetRotationAmount(rotationPerSecond, Time.fixedDeltaTime, ref targetRotation);

        RotateAroundYAxis(rotation);

        targetRotation -= rotation;
    }

    public static float GetRotationAmount(float rotationPerSecond, float timeElapsed, ref float targetRotation)
    {
        var rotation = rotationPerSecond * timeElapsed;
        if (targetRotation < 0.0f)
        {
            rotation *= -1.0f;
            if (rotation < targetRotation)
            {
                rotation = targetRotation;
            }
        }
        else if (rotation > targetRotation)
        {
            rotation = targetRotation;
        }

        return rotation;
    }
    #endregion

    #region Velocity Movement
    protected virtual void FixedUpdate()
    {
        UpdateVelocity();
    }

    protected void UpdateVelocity()
    {
        // The launch movement mode takes control over velocity
        if (CurrentMovement != null && CurrentMovement.MovementType == ActionMovement.eMovementType.LaunchToPosition)
        {
            return;
        }

        IsGrounded = Velocity.y <= Constants.Epsilon && Physics.CheckSphere(transform.position + GroundCheckSphereOffset, GroundCheckSphereRadius, BattleSystem.Instance.TerrainLayers);

        if (IsGrounded)
        {
            if (Velocity.y < Constants.Epsilon)
            {
                Velocity.y = 0.0f;
            }
        }
        else
        {
            Velocity.y += GravitationalForce * Time.deltaTime;
        }

        if (Velocity != Vector3.zero)
        {
            gameObject.transform.position += Velocity * Time.deltaTime;
        }
    }

    public void Launch(Vector3 targetPosition, float angle)
    {
        Velocity = LaunchVelocity(targetPosition, angle, 1.0f);
    }

    public Vector3 LaunchVelocity(Vector3 targetPosition, float angle, float gravityMultiplier = 1.0f)
    {
        float xzDistance = Vector3.Distance(new Vector3(transform.position.x, 0.0f, transform.position.z), new Vector3(targetPosition.x, 0.0f, targetPosition.z));
        float yDistance = Mathf.Abs(targetPosition.y - transform.position.y);
        float tanAlpha = Mathf.Tan(angle * Mathf.Deg2Rad);

        float zSpeed = Mathf.Sqrt(GravitationalForce * gravityMultiplier * xzDistance * xzDistance / (2.0f * (yDistance - xzDistance * tanAlpha)));
        float ySpeed = tanAlpha * zSpeed;

        Vector3 localVelocity = new Vector3(0f, ySpeed, zSpeed);
        return transform.TransformDirection(localVelocity);
    }

    public void SetGravitationalForce(float force)
    {
        GravitationalForce = force;
    }

    public void SetVelocity(Vector3 velocity)
    {
        Velocity = velocity;
    }
    #endregion

    #region State
    public virtual void SetRunning(bool running)
    {
        if (running)
        {
            var movementData = Entity.EntityData.Movement;
            if (movementData.ConsumeResourceWhenRunning)
            {
                var drain = movementData.RunResourcePerSecond.IncomingValue(Entity, Entity.EntityAttributes()) * Time.fixedDeltaTime;
                if (Entity.ResourcesCurrent.ContainsKey(movementData.RunResource) &&
                    Entity.ResourcesCurrent[movementData.RunResource] >= drain)
                {
                    Entity.ApplyChangeToResource(movementData.RunResource, -drain);
                    IsRunning = true;
                }
                else
                {
                    IsRunning = false;
                }
            }
        }
        else
        {
            IsRunning = false;
        }
    }
    #endregion
}
