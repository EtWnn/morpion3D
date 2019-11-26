# Morpion 3D

## Prise en Main Rapide

 1. Lancer un serveur
	 - [ ] Lancer Serveur.exe dans Serveur/Executable
	 - [ ] (optionel) changer le port et l'adresse ip du serveur. Par défault il s'agit de l'adresse local de la machine 
	 - [ ] Dans le terminal, taper sur 2 puis Entrée. Le serveur est maintenant
    disponible
	    > Note : dans le terminal, taper le numéro de la commande que vous souhaitez éxécuter
    
 
 2. Lancer un client
	- [ ] Lancer Morpion3D.exe dans Client/Executable
	- [ ] Le client se connecte automatiquement à l’adresse IP 127.0.0.1 et au port 13000. Le succès de la connexion est visible en haut à gauche de la fenêtre de jeu
	- [ ] (optionel) changer le nom d'utilisateur, le port ou l'adresse IP du serveur dans le menu option	

3. Lancer une partie entre clients
	- [ ] Avoir un serveur actif (voir 1)
	- [ ] Lancer plusieurs clients
	- [ ] Sur l’une des applications clients : 
		i.	cliquer sur Start
		ii.	cliquer sur la flèche pour rafraichir la liste des autres clients disponibles pour jouer
		iii.	cliquer sur un client disponible qui s’affiche dans la liste
		iv.	cliquer sur Send Match Request
	- [ ] Dans le client adverse choisi, cliquer sur le bouton Accept de la pop-up affichant « New Match Request » 
		> Note : Vous avez seulement quelques secondes pour accepter cette demande

4. Jouer
	- [ ] Suivez les instructions s’affichant à l’écran des applications clients pour jouer la partie
		> Note : Taper sur la barre espace pour accéder au cube central du morpion

5. Retour au Menu principal
	- [ ] Lorsqu’un des joueurs aura trouvé une combinaison gagnante, vous pourrez appuyer sur le bouton « Back to menu » pour finir la partie et revenir au menu principal

6. Fermer le jeu
	- [ ] Cliquer sur Quit sur le menu principal des applications clients
	- [ ] Taper 2 puis Entrée dans le terminal du serveur



## Architecture du code

- [ ]  **Projet Morpion3D**
	- Scripts relatifs à la visualisation dans Unity : tous les fichiers dans Assets/Scripts sauf le dossier ClientModel
	- Script relatifs à la gestion du client : tous les fichiers dans Assets/Scripts/ClientModel
		- Main script du client : Assets/Scripts/ClientModel/client.cs
		- Modélisation du Jeu du morpion 3D : tous les fichiers dans Assets/Scripts/ClientModel/ModelGame
		- Fonctions pour lire et écrire sur le Stream : le fichier Assets/Scripts/ClientModel/ModelGame/Fucntions/Messaging.cs
		- Gestion de la génération de log : le fichier Assets/Scripts/ClientModel/ModelGame/Models/LogWriter.cs
		- Model d'un user : le fichier Assets/Scripts/ClientModel/ModelGame/Models/User.cs

- [ ]   **Projet Serveur_Morpion_3D**
	- Script Principal: Serveur.cs -> gère l'interface console et le lancements du serveur
	- Gestion de la communication client: chaque client est géré par une instance de la classe UserHandler dans Models/UserHandlers.cs
	- Différents fonctions de communications utilisées dans Functions/Messaging
