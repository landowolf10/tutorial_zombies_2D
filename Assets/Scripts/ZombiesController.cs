using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiesController : MonoBehaviour
{
    public float speed = 10.0f;                 //Velocidad a la que se mueve el zombie.
    public float limiteIzquierdo;               //Límite izquierdo hasta donde puede avanzar el zombie.
    public float limiteDerecho;                 //Límite derecho hasta donde puede avanzar el zombie.
    //public float umbralVelocidad;               //
    int direccion = 1;                          //Dirección de la posición del zombie, por default está mirando hacia la derecha.
    public GameObject deadPrefab;               //Prefab que contiene el diseño del zombie desmembrado.
    public float magnitudVueloCabeza = 200f;    //Fuerza con la que saldrá volando la cabeza.

    Rigidbody2D rb;
    Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        limiteDerecho = transform.position.x + GetComponent<CircleCollider2D>().radius;
        limiteIzquierdo = transform.position.x - GetComponent<CircleCollider2D>().radius;

        //Se destruye el collider que define el radio de patrullaje para que no interfiera con otros.
        Destroy(GetComponent<CircleCollider2D>());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (rb.velocity.magnitude < umbralVelocidad)
        //{
            walk();
            flip();
        //}
            
    }

    void flip()
    {
        if (transform.position.x < limiteIzquierdo)
            direccion = 1;

        if (transform.position.x > limiteDerecho)
            direccion = -1;

        transform.localScale = new Vector3(direccion, 1, 1);
    }

    void walk()
    {
        rb.velocity = new Vector2(speed * direccion, rb.velocity.y);
    }

    public void die(Vector3 direction)
    {
        GameObject deadInstance = Instantiate(deadPrefab, transform.position, transform.rotation);

        //Se obtiene el primer y segundo hijo del objeto que contiene al zombie muerto,
        //se accede al Rigidbody2D del elemento (cabeza y cuerpo) y se le aplica una fuerza.
        deadInstance.transform.GetChild(0).GetComponent<Rigidbody2D>().AddForce(direction * magnitudVueloCabeza, ForceMode2D.Impulse);
        deadInstance.transform.GetChild(1).GetComponent<Rigidbody2D>().AddForce(direction * (magnitudVueloCabeza / 2), ForceMode2D.Impulse);
        deadInstance.transform.GetChild(1).GetComponent<Rigidbody2D>().AddTorque(10, ForceMode2D.Impulse);
        deadInstance.transform.localScale = gameObject.transform.localScale;

        Destroy(gameObject);
    }
}
