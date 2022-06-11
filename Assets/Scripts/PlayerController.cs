using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rb;
    public Transform refPie;                //Referencia del pie para saber si está pisando el suelo (es un objeto que se coloca en cualquier pie del personaje).
    public float fuerzaSalto;               //Fuerza del salto del personaje.
    public bool isGrounded;                 //Variable para saber si está tocando el suelo.
    public float horizontalVelocity;        //Campo calculado que contiene la velocidad que se le aplicará al Vector2 para desplazar al personaje.
    public float speed = 12.0f;             //Velocidad del personaje.
    public float recoilMagnitud = 300f;     //Fuerza de retroceso al disparar el arma.
    public float shakeMagnitud;             //Magnitud de la sacudida de cámara al disparar.
    public Transform weaponContainer;       //Objeto de Unity que contiene un objeto que contiene el arma, por default el arma se pone en esa posición pero se oculta.
    public Transform mira;                  //Objeto de Unity que contiene la mira.
    public Transform refManoArma;           //Objeto que apunta al limb solver que contiene la referencia de la mano derecha.
    public Transform refOjos;               //Objeto hijo del hueso de la cabeza posicionado a la altura de los ojos.
    public Transform refCabeza;             //El objeto hueso de la cabeza.
    public Transform refPuntaArma;          //Objeto que está en la punta del arma para instanciar partículas al disparar (la jerarquía es: hueso mano derecha - contenedor del arma - arma agarrada - referencia punta).
    public GameObject weaponParticles;      //Prefab de las partículas de disparo.
    public GameObject bloodParticles;       //Prefab de las partículas de sangre al disparar al cuerpo del zombie.
    public GameObject lotOfbloodParticles;  //Prefab de las partículas de sangre al disparar a la cabeza del zombie.
    bool hasWeapon = false;                 //Variable para saber si el personaje tiene el arma en la mano.
    public Transform shakeCamera;           //Objeto que guarda la referencia de la cámara virtual.
    public float shootMagnitude = 300f;     //Magnitud de disparo para empujar al zombie al dispararle.


    private Vector2 _movement; //Vector de movimiento para mover el rigidbody del personaje, es una variable calculada.
    private float horizontalInput; //Variable para detectar las teclas/botones de izquierda a derecha.

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        run();
        jump();
        flip();

        if(hasWeapon)
        {
            //Si el personaje agarró el arma, entonces la mira del arma sigue a al cursor del mouse.
            mira.position = Camera.main.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                -Camera.main.transform.position.z
            ));

            //La posición de la referencia de la mano (limb solver) gira hacia la posición de la mira.
            refManoArma.position = mira.position;

            if (Input.GetButtonDown("Fire1"))
                fire();
        }
    }

    private void FixedUpdate()
    {
        horizontalVelocity = _movement.normalized.x * speed; //Campo calculado que contiene la velocidad que se le aplicará al Vector2 para desplazar al personaje.
        rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y); //Al RigidBody2D se le aplica dicha fuerza en el eje x para desplazarse.

        //Sacudir cámara.
        if(shakeMagnitud > 0.01)
        {
            shakeCamera.rotation = Quaternion.Euler(
                Random.Range(-shakeMagnitud, shakeMagnitud),
                Random.Range(-shakeMagnitud, shakeMagnitud),
                Random.Range(-shakeMagnitud, shakeMagnitud)
            );
        }

        //Esto es para que deje de temblar.
        shakeMagnitud *= .9f;
    }

    private void LateUpdate()
    {
        if (hasWeapon)
        {
            //Girar cabeza
            refCabeza.up = refOjos.position - mira.position;

            //Arma mirando al mouse
            //weaponContainer.up = weaponContainer.position - mira.position;
        }
    }

    void run()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); //Se obtiene el botón con el que se mueve de izquierda a derecha el personaje.
        anim.SetFloat("Running", Mathf.Abs(horizontalInput)); //Se mandao el valor flotante para activar la animación de correr.
        _movement = new Vector2(horizontalInput, 0f); //Se asigna el movimiento horizontal al Vector2.
    }

    void jump()
    {
        //La posición de la referencia, el radio y layer mask, en este caso el número
        //de la capa del piso es 8.
        //Todo esto devuelve true o false.
        isGrounded = Physics2D.OverlapCircle(refPie.position, 1, 1 << 8);
        anim.SetBool("isGrounded", isGrounded);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //Si se presiona el botón de brinca y el personaje está pisando el suelo,
            //se agrega al RigidBody2D una fuerza sobre el eje y para saltar.
            rb.AddForce(new Vector2(0, fuerzaSalto), ForceMode2D.Impulse);
        }
    }

    void flip()
    {
        if(hasWeapon)
        {
            //Si tiene el arma y la posición de la mira es menor a la posición del personaje,
            //entonces gira a la izquierda.
            if (mira.transform.position.x < transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);

            //Si tiene el arma y la posición de la mira es mayor a la posición del personaje,
            //entonces gira a la derecha.
            if (mira.transform.position.x > transform.position.x)
                transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            if (horizontalInput < 0)
                transform.localScale = new Vector3(-1, 1, 1);

            if (horizontalInput > 0)
                transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void fire()
    {
        Vector3 direction = (mira.position - weaponContainer.position).normalized; //Dirección de disparo.

        //Weapon recoil.
        rb.AddForce(recoilMagnitud * -direction, ForceMode2D.Impulse);

        //Weapon particles.
        Instantiate(weaponParticles, refPuntaArma.position, Quaternion.identity);

        //Shake camera.
        shakeCameraController(0.5f);

        //Origen, dirección (punto final - origen), distancia, parámetro para que el personaje no se dispare a si mismo.
        RaycastHit2D hit = Physics2D.Raycast(weaponContainer.position, direction, 1000f, ~(1 << 9));
        
        //Si la bala le dio a algo
        if(hit.collider != null)
        {
            //Le dio al cuerpo.
            if (hit.collider.gameObject.CompareTag("Zombies"))
            {
                //Impulsar al zombie al dispararle
                hit.rigidbody.AddForce(shootMagnitude * direction, ForceMode2D.Impulse);

                //Blood particles.
                Instantiate(bloodParticles, hit.point, Quaternion.identity);
            }

            //Le dio a la cabeza.
            if (hit.collider.gameObject.CompareTag("Zombie head"))
            {
                hit.transform.GetComponent<ZombiesController>().die(direction);
                //Blood particles.
                Instantiate(lotOfbloodParticles, hit.point, Quaternion.identity);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Se le coloca un tag al objeto del arma del suelo para identificarlo.
        //Cuando el jugador entra al collider del arma con dicho tag, el arma
        //del suelo se destruye (el objeto) y el objeto del arma de la mano
        //del personaje se activa para que sea visible.
        if(collision.gameObject.CompareTag("Weapon"))
        {
            hasWeapon = true;
            Destroy(collision.gameObject);
            weaponContainer.gameObject.SetActive(true);
        }
    }

    void shakeCameraController(float maximo)
    {
        shakeMagnitud = maximo;
    }
}
