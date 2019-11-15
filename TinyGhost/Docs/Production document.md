# Production documentation

Our goal is to create a physics puzzle platformer that invokes feelings of surprise and discovery, by expanding our knowledge of Unity and Git.

### For management and organization

_Communication_ - We are using our Discord group as a primary means of communication outside of meeting.
[Here's a link.](https://discordapp.com/channels/615755920953311284/619318529380057109)  It is efficient as we all are using this system already, and it allows sending images quickly as well as text.

We meet weekly after our class on Monday (3:00 pm), then in the hour before Colloquium on Wednesday (4:00 pm).  This splits our times so that at least one IGME 601 class happens between meetings.

_Game Creation_ - We have begun development on Unity version 2019.1.10f1 as it is on all lab machines and we can get this version on our personal machines as well.  It is advantageous because Unity contains physics systems already, which is helpful for our project in specific, and it is one of the standard game engines available.  Some on our team are experienced in this area and can instruct the rest.  Additionally, the latter are receiving Unity instruction in another course at the same time, which saves on the effort of scheduling.

We intend to use Asset Store assets as well as to create our own models in Maya where feasible.  Our setting makes it such that many of the models we need are simple and geometric, with simple textures to boot.

_Documentation and Written Tasks_ - We are using Google Drive as a “scratchpad” for all written ideas, from which we bring the text for the final documentation files.  Because we can edit Google Drive documents simultaneously ("keyboard war"), we can debate ideas as a team and create written content at exactly the same time.  This is far faster than the deliberate process of “committing” changes.

Over development, we have reduced our use of Google Drive, and for good reason: we use it primarily when we have a new writing task and need to create an entire document from scratch.  Once a document has been drafted and approved by the team one time, subsequent work is faster if performed on GitHub.

_Task Management_ - We are using GitHub and GitHub Projects for storing files and performing task management, because the former is required, and also we are mostly familiar with it.  Projects, as we are learning, has a number of features that allow us to organize issues.

Management is in a series of steadily-refined drafts.  The goal is to organize user stories into short, descriptively-titled issues labeled by "Art," "Spike," and anything else we uncover during development.  Again, a first pass is done in Google Drive, but once we have identified which user stories should go in which sprint we will coalesce the information into issues on GitHub.  Then during each sprint we assign them to teammembers and update as needed.

Presently, we are using columns on Projects for "Sprint backlog," "In progress," and "Done."  There is an extra column titled "Sprint scratchpad??" which serves as a halfway point between product backlog and sprint backlog.  This is another step in refining drafts, where we pin down the broad information available in Google Drive into material we can put into action, but where we spend no more time than necessary to get the issues that will go into the present sprint.  Any surplus effort may create issue cards that will be useful for the next sprint, but it also may not, as we learn to refine based on what comes up during development.

  * [This is our repository.](https://github.com/ImBackAgain/TinyGhost) (You're already here.)
  * [This is a link to our project board.](https://github.com/ImBackAgain/TinyGhost/projects)
  * [This is the initial product backlog](https://drive.google.com/open?id=1B7tA1E94i-iAC6BEWlQPq-_f8InFJ8NFWxd5VTm5Kqk)

### Definition of Priorities

Our main objective is to build a good enough vertical slice of the final game or the minimum viable product by the end of this semester. The minimum viable product of Tiny Ghost is a playable game, though unpolished, with all the core features in place so that the player gets the feeling of how a final game would be. We defined the core features as:
- A 3D model of the tiny ghost character
- At least one full level with proper materials/textures. In our case, the level will contain
  - Bookshelves as puzzle platforms
  - At least 4 possessable objects: Eraser, Pen, Gumballs, Coin
  - Each object with its own physics-based movement and unique action
  - Books as non-possessable objects
- Win/lose conditions
- Gameplay background music
- SFX for possession, unpossession, action, win and lose
- A title screen with standard game menu options: New Game, Load Game, Settings/Controls, Exit

We defined the minimum viable interactions that a player can have with the above MVP as
- Controlling the free movement of the tiny ghost character
  - Arrow keys or W-A-S-D
- Possessing or unpossessing an object
  - Spacebar
- Ability to perform the unique action of the possessed object
  - Z key, or, practically, any unclaimed key on the keyboard
- Ability to select the game menu options

The latter is the MVI because it demonstrates the essential idea of this game: manipulating the environment and engaging in physics-based interactions on the small scale of items like a pen and eraser, while establishing that a possession mechanic is viable.  The MVP demonstrates what we believe is fun about this idea, with multiple objects as well as a play space that is believable and interesting.  The hope is that, once we demonstrate that it is fun to play with objects that have multiple forms of interaction in this physical space (e.g., a pen is very different from a coin), the MVP will establish that a more-complete game would meet our project goal.
