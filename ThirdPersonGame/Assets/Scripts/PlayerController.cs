using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{   //угол поворота, направление движения, сила прыжка, скорость поворота
    private float angleY, dirZ, jumpForce = 6f, turnSpeed = 150f;
    private bool isGrounded; //на земле или нет
    private Rigidbody rb;
    private Animator animator;
    private Vector3 jumpDir; //направление прыжка

    private GameObject sword, shield;//для хранения объектов
    private float weaponChangeTime, timeNeedToWait;
    private bool needSetWeaponActivity;

    public bool isAttacking = false;
    private GameObject enemy;

    [SerializeField]
    private Joystick joystickRight, joystickLeft;

    void Start(){
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        sword = GameObject.Find("CurrentSword");
        shield = GameObject.Find("CurrentShield");
        weaponChangeTime = 0f;
        needSetWeaponActivity = false;
        //Cursor.visible = false;

        enemy = GameObject.Find("Enemy");
    }

    void FixedUpdate(){
        angleY = joystickLeft.Horizontal * turnSpeed * Time.fixedDeltaTime;
        dirZ = joystickRight.Vertical; //Input.GetAxis("Vertical"); 
        transform.Rotate(new Vector3(0f, angleY, 0f)); //вращение игрока
    }

    private void Update(){
        isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsName("Sword_Attack_R");
        if (isGrounded){            
            animator.SetTrigger("isLanded");            
            Move(dirZ, "isWalkForward", "isWalkBack");
            Sprint();
            Dodge();      
            SetWeaponActivity();
        }
        else{
            MoveInAir();
        }
    }

    private void Move(float dir, string parametrName, string altParametrName){
        if (dir > 0.3f){
            animator.SetBool(parametrName, true);
        }
        else if (dir < -0.3f){
            animator.SetBool(altParametrName, true);
        }
        else{
            animator.SetBool(parametrName, false);
            animator.SetBool(altParametrName, false);
        }
    }

    private void Dodge(){
        if (joystickRight.Horizontal < -0.8f){
            animator.Play("Sword_Dodgle_Left");
        }
        else if (joystickRight.Horizontal > 0.8f){
            animator.Play("Sword_Dodge_Right");
        }
    }

    private void Sprint(){
        if (joystickRight.Vertical > 0.9f || joystickRight.Vertical < -0.9f){
            animator.SetBool("isRun", true);
        }
        else {
            animator.SetBool("isRun", false);
        }     
    }

    public void Jump(){
        if (isGrounded){
            animator.Play("JumpStart");
            animator.applyRootMotion = false; //откл RootMotion
            jumpDir = new Vector3(0f, jumpForce, dirZ * jumpForce / 2f);
            jumpDir = transform.TransformDirection(jumpDir);
            rb.AddForce(jumpDir, ForceMode.Impulse);
            isGrounded = false; //указываем, что уже не на земле
        }
    }

    private void MoveInAir(){
        if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude < 1.1f){
            jumpDir = new Vector3(0f, rb.velocity.y, dirZ);
            //направляем вектор вперед по соотношению игрока, а не сцены
            jumpDir = transform.TransformDirection(jumpDir);            
            rb.velocity = jumpDir;
        }  
    }

    public void PlayEquipeAnimation(){
        string stateName = "";
        float waitTime = 0f;
        if (sword.activeSelf){
            stateName = "Sword_Holster";
            waitTime = 0.5f;
        }
        else if (!sword.activeSelf){
            stateName = "Sword_Equip";
            waitTime = 0.2f;
        }
        animator.Play(stateName);//проигрываем анимацию
        weaponChangeTime = Time.time;//записываем время дейстия
        timeNeedToWait = waitTime;//указываем через сколько вызвать SetActive
        needSetWeaponActivity = true;//указываем, что теперь можем вызывать SetActive
    }

    private void SetWeaponActivity()
    {
        if(needSetWeaponActivity && (Time.time > weaponChangeTime + timeNeedToWait))
        {
            sword.SetActive(!sword.activeSelf);
            needSetWeaponActivity = false;
        }
    }

    public void Attack()
    {
        if (sword.activeSelf && isGrounded)
        {
            animator.Play("Sword_Attack_R");
        }
    }

    private void OnCollisionEnter(Collision col)
    {        
        isGrounded = true;
        animator.applyRootMotion = true;               
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Weapon"
            && enemy.GetComponent<EnemyController>().isAttacking)
        {
            animator.Play("Sword_Hit_L_2");
        }
    }
}
