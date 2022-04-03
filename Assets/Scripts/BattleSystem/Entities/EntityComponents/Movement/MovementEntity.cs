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

    public virtual bool InitiateMovement(ActionMovement movement, Entity caster = null, Entity target = null, bool isSkill = false)
    {
        if (movement.InterruptionLevel > Constants.Epsilon && movement.InterruptionLevel > Formulae.EntityInterruptResistance(Entity))
        {
            return false;
        }

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
                MovementCoroutine = StartCoroutine(MoveToPositionEnumerator(caster, target, isSkill));
                break;
            }
            case ActionMovement.eMovementType.TeleportToPosition:
            {
                Teleport(movement, caster, target);
                break;
            }
            case ActionMovement.eMovementType.LaunchToPosition:
            {
                MovementCoroutine = StartCoroutine(LaunchToPositionEnumerator(caster, target, isSkill));
                break;
            }
            case ActionMovement.eMovementType.MoveInDirection:
            {
                MovementCoroutine = StartCoroutine(MoveAlongDirectionEnumerator(caster, target, isSkill));
                break;
            }
            case ActionMovement.eMovementType.FreezePosition:
            {
                MovementCoroutine = StartCoroutine(FreezePositionCoroutine(isSkill));
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
        if (CurrentMovement == null)
        {
            return;
        }

        Velocity = Vector3.zero;
        CurrentMovement = null;

        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }

        if (Entity.EntityBattle.OnSkillCancel.Contains(CancelMovement))
        {
            Entity.EntityBattle.OnSkillCancel.Remove(CancelMovement);
        }
    }

    void UpdateActionMovementSpeed(ref float speed, float timeElapsed)
    {
        if (CurrentMovement == null)
        {
            Debug.Log("UpdateActionMovementSpeed was called, but there is no CurrentMovement set.");
            return;
        }

        speed += CurrentMovement.SpeedChangeOverTime * timeElapsed;
        if (speed < CurrentMovement.MinSpeed)
        {
            speed = CurrentMovement.MinSpeed;
        }
        else if (CurrentMovement.MaxSpeed > Constants.Epsilon && speed > CurrentMovement.MaxSpeed)
        {
            speed = CurrentMovement.MaxSpeed;
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void Teleport(ActionMovement movement, Entity caster, Entity target)
    {
        // CurrentMovement has to be set first.
        if (movement == null)
        {
            Debug.LogError($"CurrentMovement was not set.");
            return;
        }

        // Find position from transform data. 
        var found = movement.TargetPosition.TryGetTransformFromData(caster, target, out var goal, out _);
        if (!found)
        {
            Debug.LogError($"A position for movement action used by {caster.EntityUID} could not be found.");
            return;
        }

        SetPosition(goal);
        CurrentMovement = null;
    }

    public virtual IEnumerator FreezePositionCoroutine(bool isSkill)
    {
        // CurrentMovement has to be set first.
        if (CurrentMovement == null)
        {
            Debug.LogError($"CurrentMovement was not set.");
            yield break;
        }

        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Add(CancelMovement);
        }

        var startTime = BattleSystem.Time;
        do
        {
            yield return null;
        } 
        while (BattleSystem.Time - startTime > CurrentMovement.MaxDuration);

        CurrentMovement = null;
        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Remove(CancelMovement);
        }
    }

    public virtual IEnumerator MoveToPositionEnumerator(Entity caster, Entity target, bool isSkill)
    {
        // CurrentMovement has to be set first.
        if (CurrentMovement == null)
        {
            Debug.LogError($"CurrentMovement was not set.");
            yield break;
        }

        // Find position from transform data. 
        var found = CurrentMovement.TargetPosition.TryGetTransformFromData(caster, target, out var goal, out _);
        if (!found)
        {
            Debug.LogError($"A position for movement action used by {caster.EntityUID} could not be found.");
            CurrentMovement = null;
            yield break;
        }

        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Add(CancelMovement);
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

            if (CurrentMovement.HorizontalMovementOnly)
            {
                dir.y = 0.0f;
            }

            var movement = speed * elapsedTime * dir;
            if (dist < movement.sqrMagnitude)
            {
                movement = dir;
            }

            transform.position += movement;
            if (movement.sqrMagnitude > Constants.Epsilon)
            {
                if (CurrentMovement.FaceDirection == ActionMovement.eFaceDirection.FaceMovementDirection)
                {
                    transform.rotation = Quaternion.LookRotation(movement.normalized, Vector3.up);
                }
                else if (CurrentMovement.FaceDirection == ActionMovement.eFaceDirection.FaceOppositeOfMovementDirection)
                {
                    transform.rotation = Quaternion.LookRotation(-movement.normalized, Vector3.up);
                }
            }

            UpdateActionMovementSpeed(ref speed, elapsedTime);
            yield return null;
        }
        while (dist > Constants.Epsilon && totalElapsedTime < CurrentMovement.MaxDuration);

        CurrentMovement = null;
        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Remove(CancelMovement);
        }
    }

    public virtual IEnumerator LaunchToPositionEnumerator(Entity caster, Entity target, bool isSkill)
    {
        // CurrentMovement has to be set first.
        if (CurrentMovement == null)
        {
            Debug.LogError($"CurrentMovement was not set.");
            yield break;
        }

        // Find position from transform data. 
        var found = CurrentMovement.TargetPosition.TryGetTransformFromData(caster, target, out var goal, out _);
        if (!found)
        {
            Debug.LogError($"A position for movement action used by {Entity.EntityUID} could not be found.");
            CurrentMovement = null;
            yield break;
        }

        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Add(CancelMovement);
        }

        // Rotate toward target position if required.
        var dir = goal - Entity.transform.position;
        dir.y = 0.0f;
        if (dir.sqrMagnitude > Constants.Epsilon)
        {
            if (CurrentMovement.FaceDirection == ActionMovement.eFaceDirection.FaceMovementDirection)
            {
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
            else if (CurrentMovement.FaceDirection == ActionMovement.eFaceDirection.FaceOppositeOfMovementDirection)
            {
                transform.rotation = Quaternion.LookRotation(-dir.normalized, Vector3.up);
            }
        }

        var startTime = BattleSystem.Time;
        float elapsedTime;
        float lastTime = startTime;

        var gravityMultiplier = CurrentMovement.Speed > Constants.Epsilon ? CurrentMovement.Speed : 1.0f;
        var gravity = GravitationalForce * gravityMultiplier;
        Velocity = LaunchVelocity(goal, CurrentMovement.LaunchAngle, gravityMultiplier);

        float dist = (new Vector2(transform.position.x, transform.position.z) - new Vector2(goal.x, goal.z)).sqrMagnitude;
        var maxDist = Entity.EntityData.Radius * Entity.EntityData.Radius;

        var launched = false;
        var land = false;

        // Move toward position.
        do
        {
            elapsedTime = BattleSystem.Time - lastTime;
            lastTime = BattleSystem.Time;

            gameObject.transform.position += Velocity * elapsedTime;

            dist = (new Vector2(transform.position.x, transform.position.z) - new Vector2(goal.x, goal.z)).sqrMagnitude;

            Velocity.y += gravity * elapsedTime;

            if (!launched)
            {
                launched = !IsGrounded;
            }

            land = launched && (IsGrounded || dist < maxDist);

            yield return null;
        }
        while (!land);

        Velocity = Vector3.zero;
        CurrentMovement = null;
        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Remove(CancelMovement);
        }
    }
    public virtual IEnumerator MoveAlongDirectionEnumerator(Entity caster, Entity target, bool isSkill)
    {
        // CurrentMovement has to be set first.
        if (CurrentMovement == null)
        {
            Debug.LogError($"CurrentMovement was not set.");
            yield break;
        }

        // Find forward direction.
        var found = CurrentMovement.TargetPosition.Direction.TryGetDirectionFromData(caster, target, out var direction);
        if (!found)
        {
            Debug.LogError($"A position for movement action used by {Entity.EntityUID} could not be found.");
            CurrentMovement = null;
            yield break;
        }

        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Add(CancelMovement);
        }

        var startTime = BattleSystem.Time;
        float totalElapsedTime;
        float elapsedTime;
        float lastTime = startTime;

        if (CurrentMovement.HorizontalMovementOnly)
        {
            direction.y = 0.0f;
        }
        direction.Normalize();

        if (direction.sqrMagnitude > Constants.Epsilon)
        {
            if (CurrentMovement.FaceDirection == ActionMovement.eFaceDirection.FaceMovementDirection)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
            else if (CurrentMovement.FaceDirection == ActionMovement.eFaceDirection.FaceOppositeOfMovementDirection)
            {
                transform.rotation = Quaternion.LookRotation(-direction.normalized, Vector3.up);
            }
        }

        var speed = (CurrentMovement.Speed > Constants.Epsilon ? CurrentMovement.Speed :
                     Formulae.EntityMovementSpeed(Entity, false));

        // Move toward position.
        do
        {
            totalElapsedTime = BattleSystem.Time - startTime;
            elapsedTime = BattleSystem.Time - lastTime;
            lastTime = BattleSystem.Time;

            Move(elapsedTime * speed * direction, faceMovementDirection: false, setMovementTrigger: false);

            UpdateActionMovementSpeed(ref speed, elapsedTime);
            yield return null;
        }
        while (totalElapsedTime < CurrentMovement.MaxDuration);

        CurrentMovement = null;
        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Remove(CancelMovement);
        }
    }
    #endregion

    #region Action Rotation
    public ActionRotation CurrentRotation { get; protected set; }
    Coroutine RotationCoroutine;

    public virtual bool InitiateRotation(ActionRotation rotation, Entity caster = null, Entity target = null, bool isSkill = false)
    {
        if (rotation.InterruptionLevel > Constants.Epsilon && rotation.InterruptionLevel > Formulae.EntityInterruptResistance(Entity))
        {
            return false;
        }

        if (CurrentRotation != null)
        {
            if (CurrentRotation.Priority > rotation.Priority)
            {
                return false;
            }
            else
            {
                CancelRotation();
            }
        }

        CurrentRotation = rotation;

        switch (rotation.RotationType)
        {
            case ActionRotation.eRotationType.SetRotation:
            {
                SetRotation(caster, target);
                break;
            }
            case ActionRotation.eRotationType.Rotate:
            {
                RotationCoroutine = StartCoroutine(RotateCoroutine(isSkill));
                break;
            }
            case ActionRotation.eRotationType.RotateToDirection:
            {
                RotationCoroutine = StartCoroutine(RotateTowardDirectionCoroutine(caster, target, isSkill));
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented rotation type: {rotation.RotationType}");
                return false;
            }
        }

        return true;
    }

    public virtual void CancelRotation()
    {
        CurrentRotation = null;
        if (RotationCoroutine != null)
        {
            StopCoroutine(RotationCoroutine);
        }

        if (Entity.EntityBattle.OnSkillCancel.Contains(CancelRotation))
        {
            Entity.EntityBattle.OnSkillCancel.Remove(CancelRotation);
        }
    }


    void UpdateActionRotationSpeed(ref float speed, float timeElapsed)
    {
        if (CurrentRotation == null)
        {
            Debug.Log("UpdateActionRotationSpeed was called, but there is no CurrentRotation set.");
            return;
        }

        speed += CurrentRotation.SpeedChangeOverTime * timeElapsed;
        if (speed < CurrentRotation.MinSpeed)
        {
            speed = CurrentRotation.MinSpeed;
        }
        else if (CurrentRotation.MaxSpeed > Constants.Epsilon && speed > CurrentRotation.MaxSpeed)
        {
            speed = CurrentRotation.MaxSpeed;
        }
    }

    public void SetRotation(Entity caster, Entity target)
    {
        // CurrentRotation has to be set first.
        if (CurrentRotation == null)
        {
            Debug.LogError($"CurrentRotation was not set.");
            return;
        }

        // Find target rotation.
        var found = CurrentRotation.Direction.TryGetDirectionFromData(caster, target, out var targetDirection);
        if (!found)
        {
            Debug.LogError($"A direction for rotation action used by {caster.EntityUID} could not be found.");
            CurrentRotation = null;
            return;
        }

        FaceDirection(targetDirection);
        CurrentRotation = null;
    }

    public IEnumerator RotateCoroutine(bool isSkill)
    {
        // CurrentRotation has to be set first.
        if (CurrentRotation == null)
        {
            Debug.LogError($"CurrentRotation was not set.");
            yield break;
        }

        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Add(CancelRotation);
        }

        var rotationSpeed = (CurrentRotation.Speed > Constants.Epsilon || CurrentRotation.Speed < -Constants.Epsilon) ?
                            CurrentRotation.Speed : Formulae.EntityRotateSpeed(Entity);

        var lastTime = BattleSystem.Time;
        float elapsedTime;
        var totalElapsedTime = 0.0f;

        do
        {
            elapsedTime = BattleSystem.Time - lastTime;
            totalElapsedTime += elapsedTime;

            RotateAroundYAxis(rotationSpeed * elapsedTime);
            UpdateActionRotationSpeed(ref rotationSpeed, elapsedTime);

            yield return null;
        }
        while (totalElapsedTime < CurrentRotation.MaxDuration);

        CurrentRotation = null;
        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Remove(CancelRotation);
        }
    }

    public IEnumerator RotateTowardDirectionCoroutine(Entity caster, Entity target, bool isSkill)
    {
        // CurrentRotation has to be set first.
        if (CurrentRotation == null)
        {
            Debug.LogError($"CurrentRotation was not set.");
            yield break;
        }

        // Find target rotation.
        var found = CurrentRotation.Direction.TryGetDirectionFromData(caster, target, out var targetDirection);
        if (!found)
        {
            Debug.LogError($"A direction for rotation action used by {caster.EntityUID} could not be found.");
            CurrentRotation = null;
            yield break;
        }

        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Add(CancelRotation);
        }

        var rotationSpeed = (CurrentRotation.Speed > Constants.Epsilon || CurrentRotation.Speed < -Constants.Epsilon) ? 
                            CurrentRotation.Speed : Formulae.EntityRotateSpeed(Entity);
        var currentSpeed = 0.0f;

        targetDirection.y = 0.0f;

        var startTime = BattleSystem.Time;
        var elapsedTime = 0.0f;
        var lastTime = startTime;
        var totalElapsedTime = 0.0f;

        do
        {
            elapsedTime = BattleSystem.Time - lastTime;
            totalElapsedTime = BattleSystem.Time - startTime;

            currentSpeed = elapsedTime * rotationSpeed;

            var direction = Vector3.RotateTowards(transform.forward, targetDirection, currentSpeed, 0.0f);
            FaceDirection(direction);

            UpdateActionRotationSpeed(ref rotationSpeed, elapsedTime);
            lastTime = BattleSystem.Time;
            yield return null;
        }
        while (totalElapsedTime < CurrentRotation.MaxDuration && Vector3.Angle(transform.forward, targetDirection) > currentSpeed);
        FaceDirection(targetDirection);

        CurrentRotation = null;
        if (isSkill)
        {
            Entity.EntityBattle.OnSkillCancel.Remove(CancelRotation);
        }
    }
    #endregion

    #region Movement
    public bool ObstacleCheck(Vector3 direction)
    {
        var ray = new Ray(transform.position + Vector3.up * Constants.ObstacleDetectHeight, direction);
        var rayLength = Entity.EntityData.Radius + Constants.ObstacleDetectRange;
        return Physics.Raycast(ray, rayLength, BattleSystem.Instance.TerrainLayers);
    }

    public virtual void Move(Vector3 movement, bool faceMovementDirection, bool setMovementTrigger)
    {
        transform.position += movement;
        if (faceMovementDirection && movement.sqrMagnitude > Constants.Epsilon)
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
        if ((!ignoreLock && Entity.IsMovementLocked) || Entity.EntityData.Movement.MovementSpeed < Constants.Epsilon || ObstacleCheck(direction))
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
        var wasGrounded = IsGrounded;
        IsGrounded = Velocity.y <= Constants.Epsilon && Physics.CheckSphere(transform.position + GroundCheckSphereOffset, GroundCheckSphereRadius, BattleSystem.Instance.TerrainLayers);

        var landed = !wasGrounded && IsGrounded;
        if (landed)
        {
            Entity.OnEntityLanded();
        }

        // The launch movement mode takes control over velocity
        if (CurrentMovement != null && (CurrentMovement.MovementType == ActionMovement.eMovementType.LaunchToPosition || 
            CurrentMovement.MovementType == ActionMovement.eMovementType.FreezePosition))
        {
            if (landed)
            {
                CancelMovement();
            }
            else
            {
                return;
            }
        }

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
        if (xzDistance < Constants.Epsilon)
        {
            Debug.Log("xz distance is too small to launch.");
            return Vector3.zero;
        }

        float yDistance = Mathf.Abs(targetPosition.y - transform.position.y);
        float tanAlpha = Mathf.Tan(angle * Mathf.Deg2Rad);

        float zSpeed = Mathf.Sqrt(GravitationalForce * gravityMultiplier * xzDistance * xzDistance / (2.0f * (yDistance - xzDistance * tanAlpha)));
        if (float.IsNaN(zSpeed))
        {
            Debug.Log("zSpeed is not a number.");
            return Vector3.zero;
        }
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
