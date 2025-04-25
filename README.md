# 🎮 Entregable 1 - Programación de Videojuegos III
## Alumno: Matías Lorenzo
---
Todos los archivos que se utilizaron en blender y unity se encuntran dentro de la carpeta Assets/TP1, salvo excepción que se comentará luego.
Este proyecto consiste en la transformación de una escena de una entrega anterior, cumpliendo con los requisitos solicitados y que se detallarán a continuación:

---

## 📑 Índice

1. [Iluminación: Baked, Mixed y Real-Time](#1-iluminación-baked-mixed-y-real-time)
2. [Punto de vista en Primera Persona](#2-punto-de-vista-en-primera-persona)
3. [Implementación de objetos con LOD](#3-implementación-de-objetos-con-lod)
4. [Animación en loop e interacción](#4-animación-en-loop-e-interacción)

---

## 1. Iluminación: Baked, Mixed y Real-Time

En esta escena se aplicaron distintas técnicas de iluminación según el tipo de objeto y su interacción con el jugador:

- **Baked Lighting** se utilizó para objetos estáticos como paredes y suelos, mejorando el rendimiento al precalcular la iluminación.
- **Mixed Lighting** se aplicó en objetos que están en contacto con fuentes de luz dinámicas pero que no se mueven.
- **Real-Time Lighting** fue usado para luces en objetos que reaccionan al jugador o que se encienden/apagan durante la ejecución del juego.

📸 *Ejemplo visual de iluminación combinada:*

![Iluminación Baked, Mixed y Real-Time](ruta/a/imagen1.png)

---

## 2. Punto de vista en Primera Persona

Para pasar a primera persona, se tuvo que agregar una camara cercana a su cara, modificar archivos del PlayerController.cs para controlar el movimiento de la camara con el mouse, adaptarse el collider par que no atravesa paredes y columnas.

📸 *Captura de vista en primera persona:*

![First Person View](ruta/a/imagen2.png)

---

## 3. Implementación de objetos con LOD

Se incorporaron dos objetos con **niveles de detalle (LOD)** para optimizar el rendimiento gráfico Se encuentran dentro de "/Assets/TP1/Modelos LOD". Dependiendo de la distancia del jugador, los modelos cambian automáticamente su nivel de complejidad visual. Se remplazaron esos mismos objetos en todo el nivel.

- LOD0: Detalle completo (cerca)
- LOD1: Menor detalle (media distancia)
- LOD2: Silueta simple (lejos)

📸 *Visualización de transiciones LOD:*

![Objetos con LOD](ruta/a/imagen3.png)

---

## 4. Animación en loop e interacción

Se animaron dos objetos en blender para dar vida al entorno: una llave con un loop de moviendose necesaria para abrir una puerta (que también tiene una animación de abrirse). Se encuentran en "/Assets/TP1/Modelos Animados"

📸 *Animación en loop con interacción:*

![Animación en loop e interacción](ruta/a/imagen4.png)

---

## 📌 Autor
*Matías Lorenzo*  
*Materia: Programación de Videojuegos III*  
*Profesor: Cristian Reynaga*  
*2025 1C*