# Infiltrator
A sandbox project which try to recreate "VR Training Metal Gear Solid" with some tests on Lightmapping and Light Probes with Unity

Technologie : Unity3D 2019.4 (C#)

Projet "Sandbox" réalisé avec Unity3D, commencé en 2014 et mise à jour en 2020 (conception et façon de programmer un peu désuet...), pour tenter de recréer des mécaniques de "VR Training Metal Gear Solid" 

Le but était de créer une IA simple (Patrol / Static / Ping Pong) avec le système de Navigation de Unity.

Le comportement de celle-ci est la suivante :

Static / Patrol / Ping-Pong
Bruit suspect (course du personnage / cogner sur les murs) -> Alerte avec déplacement sur la zone
Joueur en visuel -> Attaque et poursuite de celui-ci
Déplacement des soldats jusqu'au dernier endroit où le joueur a été vu
Mode vigie (plusieurs rotations sur lui-même)
Retour à l'activité de base (Static / Patrol / Ping-Pong) 


En ce qui concerne la création du niveau, des primitives ont été utilisé tout simplement mais le but était d'avoir un éclairage intéressant.

D'où l'utilisation du Deferred Rendering Path et de la création de plusieurs Lightmaps.

Unity gère cela très bien et permet d'avoir un rendu très convainquant (même si on est loin de Unreal).



Utilisation des modèles 3D fournis par Unity

Création de l'arbre d'Animation avec Animator (Mecanim)

Création du pathfinding avec les NavMesh

Création du lighting (Baked) avec l'outil de Unity

Création du lighting (Realtime) avec l'ajout de Light Probes

