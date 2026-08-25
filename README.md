# Custom Vehicle Physics

A custom vehicle physics system built in Unity and C#, focused on understanding and implementing vehicle physics from the ground up.

![Custom Vehicle Physics](VehiclePhysics-BackDrop.png)

## Portfolio

Full project breakdown, technical details, screenshots, and development information:

https://kaushal-portfolio-liart.vercel.app/Projects/vehicle-controller

## Video Demo

Watch the vehicle physics demonstration on YouTube:

https://youtu.be/7bqbi1CSBPA?si=DDCUQ0w8ghMFl5aK

---

## Why I Made It

I started working on this project to understand vehicle physics at a deeper level.

I've seen vehicle physics implemented in games like GTA, Saints Row, and other driving games, and I wanted to understand what actually happens underneath the surface.

Rather than relying entirely on Unity's built-in PhysX Rigidbody and collision systems, I wanted to explore how much of the vehicle physics I could build and control myself.

---

## Where Did the Idea Come From?

I had an interview in late 2025 where I was given a task to create a Forza Horizon-style vehicle controller using Unity's Character Controller and Raycasts within 24 hours.

The task involved implementing:

- Wheel suspension using Raycasts
- Cinematic camera
- Skidding
- Vehicle sounds
- Collision handling
- Vehicle animation

The project was extremely challenging, but it taught me a lot about what I wanted to explore next in my game programming journey.

That experience eventually became the foundation for this project.

---

## The Interesting Part

This isn't just a basic car controller.

The system explores more advanced vehicle physics concepts, including:

- Pacejka-inspired tire modeling
- Friction
- Traction
- Lateral tire forces
- Longitudinal tire forces
- Suspension
- Linear and rotational chassis movement
- Slip force calculations
- Slip angle calculations

The architecture is data-driven, with responsibilities separated across different systems.

This allows different vehicle configurations to share the same underlying physics framework while keeping the system easier to maintain and extend.

---

## Custom Wheel Physics

The vehicle does not rely on Unity's built-in `WheelCollider` system.

Custom wheel physics are used to determine wheel-ground interaction and calculate the forces acting on the vehicle.

The wheel system handles:

- Ground detection
- Contact points
- Surface normals
- Suspension behavior
- Wheel forces
- Wheel-ground interaction

A `SphereCast` is used for wheel-ground detection, providing the information required by the physics system to determine how each wheel should respond to the surface.

---

## Suspension

The suspension system calculates the displacement of each wheel relative to the vehicle and uses this information to apply forces to the vehicle body.

This provides direct control over:

- Suspension travel
- Wheel-ground interaction
- Suspension response
- Vehicle body movement

---

## Curb Detection

The system also handles uneven geometry such as road curbs.

Surface normals are evaluated using the dot product between the detected surface normal and the vehicle's up direction
