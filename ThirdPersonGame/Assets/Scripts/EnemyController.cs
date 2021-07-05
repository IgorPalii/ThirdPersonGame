using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private int health;
    private float prevHitTime, ignoreDamageWindow;
    private Animator animator;

    private NavMeshAgent agent; //объект для компонента 
    private Transform playerT; //компонент трансформ игрока
    //время предыдущей атаки, сколько ждать до следующей
    private float prevAttackTime, pauseAttackWindow;
    [SerializeField] //точки по которым враг патрулирует 
    private Transform[] patrolTargets;
    //индекс точки, к которой нужно двигаться
    private int currentTargetIndex = 0; 
    public bool isAttacking = false;//атакует ли враг

    private void Start(){
        agent = GetComponent<NavMeshAgent>();
        playerT = GameObject.Find("Character").transform;
        pauseAttackWindow = 2.5f;

        animator = GetComponent<Animator>();
        health = 3;
        prevHitTime = 0f;
        ignoreDamageWindow = 1.5f;
    }

    private void Update() {
        isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsName("Sword_Attack_R");
        if (health > 1) {
            float distanceToPlayer = Vector3.Distance(transform.position, playerT.position);
            if (distanceToPlayer < 2.5f) {
                Attack();
            }
            else if (distanceToPlayer > 30f) {
                PatrolBehaviour();
            }
            else {
                MoveToPlayer();
            }            
        }
    }

    private void OnTriggerEnter(Collider col){
        if (col.gameObject.tag == "Weapon" 
        && playerT.gameObject.GetComponent<PlayerController>().isAttacking 
        && Time.time > prevHitTime + ignoreDamageWindow) {
            health--;
            prevHitTime = Time.time;
            if(health > 1){
                animator.Play("KnockdownRight");
            }
            else if(health == 1){
                animator.Play("Sword_Defeat_2_Start");
            }
            else{
                animator.SetTrigger("isDead");
            }
        }
    }

    private void CheckNewPatrolTarget() {
        Vector3 targetPos = patrolTargets[currentTargetIndex].position;
        if (Vector3.Distance (transform.position, targetPos) < 0.5f) {
            if (currentTargetIndex < patrolTargets.Length -1) {
                currentTargetIndex++;//если индекс не последний в масиве
            }                        //устанавливаем индекс на 1 больше
            else {
                currentTargetIndex = 0;//иначе 0
            }
        }
    }

    private void MoveToPlayer()
    {
        animator.SetBool("isWalk", true);
        agent.destination = playerT.position;
    }

    private void PatrolBehaviour()
    {//если есть точки патрулирования в массиве
        if (patrolTargets.Length > 0)
        {
            animator.SetBool("isWalk", true);//включаем анимацию ходьбы
            agent.destination = patrolTargets[currentTargetIndex].position;
            CheckNewPatrolTarget();
        }
    }

    private void Attack() {
        animator.SetBool("isWalk", false);//отключаем анимацию ходьбы
        agent.destination = transform.position;
        transform.LookAt(playerT.position);//поварачиваем к игроку лицом
        if (Time.time > prevAttackTime + pauseAttackWindow
            && !animator.GetCurrentAnimatorStateInfo(0).IsName("KnockdownRight"))
        {
            animator.Play("Sword_Attack_R");
            prevAttackTime = Time.time;
        }
    }
}
