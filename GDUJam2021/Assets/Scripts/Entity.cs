using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class Entity : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float movementSpeed;
    [SerializeField, Range(0, 1f), Tooltip("How fast the Entity stops moving when there is no input")]
                     protected float stoppingPower = 0.125f;
    [SerializeField] protected float jumpForce;
    [SerializeField, Range(0, 0.4f)]
                     protected float jumpGracePeriod = 0.1f;
    [SerializeField] protected bool canDoubleJump = false;
    [SerializeField, Range(0, 2)] 
                     protected float airControlPercent = 0.45f;
    [SerializeField] protected bool useGravity;
    [SerializeField, Tooltip("This is only to show whether the entity is grounded. Can not modify.")] 
                     internal bool isGrounded;
    [SerializeField] protected float normalGravity = 1, fallGravity = 2;

    [Header("Health")]
    [SerializeField] protected int maxHealth;
    [SerializeField] internal int currentHealth;
    [SerializeField] internal float invulnerabilityTime = 0.15f;
    [SerializeField] internal EntityState currentEntityState = EntityState.Active;
    [SerializeField] internal AudioClip hitSound, deathSound;

    [Header("Ground Check")]
    [SerializeField] protected GameObject groundCheckLeft;
    [SerializeField] protected GameObject groundCheckRight;
    [SerializeField] protected GameObject groundCheckMiddle;
    [SerializeField] protected LayerMask groundMask, enemyMask;
    internal float groundCheckDistance = 0.1f;
    internal int internalSpeedMultiplier = 1000;

    [Header("Other")]
    [SerializeField] protected GameObject GFX;
    [SerializeField] internal SpriteRenderer spriteRenderer;
    [SerializeField, ColorUsage(true, true)] public Color normalColor;
    [SerializeField, ColorUsage(true, true)] public Color heldColor;
    [SerializeField, ColorUsage(true, true)] public Color deadColor;
    [SerializeField, ColorUsage(true, true)] public Color damageColor;

    internal AudioSource audioSource;
    internal Rigidbody2D rb;
    internal Collider2D col;
    internal Animator anim;
    internal Transform initialTransform;
    internal EntityState initialState;
    internal Vector2 input, aim, movementVector;
    internal bool canMove; // Might remove later, useless now
    internal bool canTakeDamage;

    internal float currentMovementMultiplier;
    internal float jumpTimeElapsed;
    internal bool _isGrounded => CheckIsGrounded();
    internal bool canJump => _isGrounded || jumpTimeElapsed >= 0;
    internal bool isCollided, hasJumped, hasDoubleJumped;
    internal float totalSpeedMultiplier => movementSpeed * currentMovementMultiplier * internalSpeedMultiplier;
    internal int sign = 1;

    internal virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupEntity();
    }

    internal virtual void FixedUpdate()
    {
        Move();
    }

    internal void SetMovementVector()
    {
        movementVector = useGravity ? new Vector2(input.x, 0).normalized : input;
        if (movementVector.x > 0) sign = 1;
        else if (movementVector.x < 0) sign = -1;
    }

    internal void ResetVelocityAndInput()
    {
        input = Vector2.zero;
        rb.velocity = Vector2.zero;
    }

    internal virtual void Move()
    {
        isGrounded = _isGrounded;
        if (IsActive())
        {
            jumpTimeElapsed = isGrounded ? jumpGracePeriod : jumpTimeElapsed - Time.deltaTime;
            if (input != Vector2.zero)
            {
                currentMovementMultiplier = isGrounded ? 1 : airControlPercent;
                rb.AddForce(movementVector * totalSpeedMultiplier * Time.deltaTime, ForceMode2D.Force);
            }
            else
            {
                rb.AddForce(-rb.velocity.x * Vector2.right * totalSpeedMultiplier * stoppingPower * Time.deltaTime);
            }
            if(useGravity)
            {          
                rb.gravityScale = (rb.velocity.y < 0f) ? fallGravity : normalGravity;
            }
        }
    }

    internal virtual void Jump()
    {
        Vector2 doubleJumpBoost = Vector2.up;
        if (!IsActive()) 
            return;
        // Jump
        if (canJump && !hasJumped) 
        {
            hasJumped = true;
            rb.AddForce(Vector2.up * jumpForce * 10, ForceMode2D.Impulse);
        }
        // Double Jump
        else if (canDoubleJump && hasJumped && !hasDoubleJumped)
        {
            // Jump boost for if the player is falling
            if (rb.velocity.y <= 0.1f) doubleJumpBoost = doubleJumpBoost + Vector2.up * Mathf.Clamp01(Mathf.Abs(rb.velocity.y));
            hasDoubleJumped = true;
            rb.AddForce(jumpForce * doubleJumpBoost * 8, ForceMode2D.Impulse);
        }
    }


    /// <summary>
    /// The initial setup of the Entity
    /// </summary>
    internal virtual void SetupEntity()
    {
        currentHealth = maxHealth;
        initialTransform = transform;
        initialState = currentEntityState;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        col = GetComponent<Collider2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (col != null) col.isTrigger = false;
        audioSource = GetComponent<AudioSource>();
        SetEntityState();
    }

    /// <summary>
    /// Resets the entity to its initial state: health, position, and EntityState
    /// </summary>
    internal virtual void ResetEntity()
    {
        currentHealth = maxHealth;
        transform.position = initialTransform.position;
        transform.rotation = initialTransform.rotation;
        currentEntityState = initialState;
        if (col != null) col.isTrigger = false;
        SetEntityState();
    }

    internal virtual void SetEntityState()
    {
        switch (currentEntityState)
        {
            case EntityState.Active:
                {
                    canMove = true;
                    SetVulnerable();
                }
                break;
            case EntityState.Inactive:
                {
                    canMove = false;
                    SetInvulnerable();
                }
                break;
            case EntityState.Dead:
                {
                    canMove = false;
                    SetInvulnerable();
                }
                break;
            default:
                {
                    canMove = true;
                }
                break;
        }
    }

    /// <summary>
    /// Returns true if the currentEntityState is Active
    /// </summary>
    internal bool IsActive()
    {
        return currentEntityState == EntityState.Active;
    }

    #region Health

    /// <summary>
    /// Damages the Entity and optionally sets it invulnerable for a time based on the hurt animation
    /// </summary>
    /// <param name="dmg"></param>
    /// <param name="setInvulnerable"></param>
    internal virtual void Damage(int dmg, bool setInvulnerable = true)
    {
        if (canTakeDamage)
        {
            currentHealth -= dmg;
            if(currentHealth <= 0)
            {
                currentHealth = 0;
                Kill();
            }
            // TODO - Sound effect here
            if(setInvulnerable)
            {
                SetInvulnerable();
                Invoke("SetVulnerable", invulnerabilityTime);
            }
        }
    }
    /// <summary>
    /// Heals the Entity
    /// </summary>
    /// <param name="heal"></param>
    internal virtual void Heal(int heal)
    {
        currentHealth += heal;
        if (currentHealth >= maxHealth)
            currentHealth = maxHealth;
        // TODO - Sound effect here
    }
    /// <summary>
    /// Kills the Entity
    /// </summary>
    internal virtual void Kill()
    {
        // TODO - Kill the Entity
        currentEntityState = EntityState.Dead;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        spriteRenderer.DOColor(deadColor, invulnerabilityTime / 2f);
        if (col != null) col.isTrigger = true;
        // TODO - Sound effect here
        audioSource.PlayOneShot(deathSound);
        CancelInvoke();
        Destroy(gameObject, 0.5f);
    }
    internal virtual void SetInvulnerable()
    {
        canTakeDamage = false;
        spriteRenderer.DOColor(damageColor, invulnerabilityTime / 2f);
        // TODO - Animation
    }
    internal virtual void SetVulnerable()
    {
        canTakeDamage = true;
        spriteRenderer.DOColor(normalColor, invulnerabilityTime / 3f);
    }

    #endregion


    /// <summary>
    /// Returns true if the Entity is grounded. 
    /// The entity is grounded if at least one of its ground checks returns true.
    /// </summary>
    internal bool CheckIsGrounded()
    {
        if (groundCheckLeft == null || groundCheckRight == null || groundCheckMiddle == null)
        {
            Debug.LogError(transform.name + ": No Ground Check Object!");
            return false;
        }
        RaycastHit2D hitL = Physics2D.Raycast(groundCheckLeft.transform.position, Vector2.down, groundCheckDistance, groundMask);
        RaycastHit2D hitR = Physics2D.Raycast(groundCheckRight.transform.position, Vector2.down, groundCheckDistance, groundMask);
        RaycastHit2D hitM = Physics2D.Raycast(groundCheckMiddle.transform.position, Vector2.down, groundCheckDistance, groundMask);
        if (hitR || hitL || hitM)       
            return true;     
        return false;
    }

    internal virtual void OnCollisionEnter2D(Collision2D collision)
    {
        isCollided = true;
        if (_isGrounded)
        {
            hasJumped = false;
            hasDoubleJumped = false;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        isCollided = true;
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        isCollided = false;
    }
    internal void RotateGFX(Vector2 dir)
    {
        if (GFX == null) return;
        if (dir.x < 0)
        {
            GFX.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if(dir.x > 0)
        {
            GFX.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
    private void OnDrawGizmos()
    {
        if (groundCheckLeft == null || groundCheckRight == null || groundCheckMiddle == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheckRight.transform.position, groundCheckRight.transform.position + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(groundCheckLeft.transform.position, groundCheckLeft.transform.position + Vector3.down * groundCheckDistance);
    }
}

public enum EntityState
{ 
    Active, Inactive, Dead
}