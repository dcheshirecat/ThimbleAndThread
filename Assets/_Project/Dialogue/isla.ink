// Isla Marlowe: keeper of the old lighthouse. Adult; romanceable. Arrives in chapter 2.
// Dry, gruff, secretly hopeless at knitting. Friendship scenes at levels 1-3; at level 3 the player can
// ask her out (~ romance(1)), and scenes 4 and 5 follow the romance if they did.

=== isla_visit ===
Isla: You're the new one. The great-niece. # neutral
Isla: I'm Isla. The lighthouse. Well, not the lighthouse. I keep it.
-> END

=== isla_visit_3 ===
Isla: Thought I'd see if you'd eaten. You haven't. I can tell. # thoughtful
-> END

=== isla_request_wind ===
# fits: sweater, scarf
# best: sweater
Isla: Wind comes off the water like it's got a grudge. Up the top of the tower it's worse. # neutral
Isla: I need something thick. Something a gale would give up on.
-> END

=== isla_request_window ===
# fits: hoop_art, sampler, tablecloth
# best: hoop_art
Isla: The keeper's cottage has one window, and it looks at a wall. # thoughtful
Isla: I'd like something to look at that isn't the sea. Love the sea. Sick of the sea.
-> END

=== isla_handover_good ===
Isla: Huh. # surprised
Isla: That's... really good. Don't let it go to your head. # happy
-> END

=== isla_handover_okay ===
Isla: Does the job. I mean that kindly. # neutral
-> END

=== isla_handover_poor ===
Isla: Well. The gulls won't care. # thoughtful
-> END

=== isla_scene_1 ===
Isla: Confession. # thoughtful
She puts something on the counter. It might once have been intended as a sock.
Isla: Three winters I've been trying. The pattern says "simple". The pattern is a liar.
* [Laugh] You laugh. She glares, and then her mouth twitches.
    Isla: Fine. It's funny. It's very funny. # happy
    ~ friendship(5)
* [Let me look at it.]
    Isla: You'd do that? Most people just say "oh, how nice". # surprised
    ~ friendship(5)
- Isla: Come up the tower some evening. I'll trade you the view for a lesson.
-> END

=== isla_scene_2 ===
The lighthouse stairs go round and round and round. At the top, Isla is waiting with two mugs.
Isla: Hundred and twelve steps. You didn't even complain. I'm impressed. # happy
Isla: Look. That's the whole town. Your shop's the one with the light still on. It always is.
* [It's beautiful up here.]
    Isla: It is. I don't say that to many people. # thoughtful
    ~ friendship(5)
* [Do you ever get lonely?]
    Isla: ... Yes. # sad
    Isla: Less, lately.
    ~ friendship(10)
-> END

=== isla_scene_3 ===
Isla: The harbour lights festival's next week. Lanterns on the water. Daft, really. # thoughtful
Isla: Everyone goes with someone.
* [Would you like to go with me?]
    Isla: ... # surprised
    Isla: Yes. I would. I really would. # happy
    ~ romance(1)
    ~ friendship(10)
* [We should all go together, the whole shop crowd!]
    Isla: Sure. Course. That sounds fun. # neutral
    ~ friendship(5)
-> END

=== isla_scene_4 ===
* {romance} [Walk along the harbour]
    Lanterns bob on the black water. Isla's hand finds yours and doesn't let go.
    Isla: I knitted you something. Don't look at it too closely. # thoughtful
    It's a scarf. Mostly. It has one perfect stitch in the middle, as if she practised that one a hundred times.
    Isla: That one's for you. The rest are for the wind. # happy
    ~ friendship(10)
* {not romance} [Share a pot of tea]
    Isla: You're a good friend, you know. The best one I've had in a long time. # happy
    ~ friendship(5)
-> END

=== isla_scene_5 ===
* {romance} [Climb the tower at dusk]
    The lamp turns above you, sweeping the bay.
    Isla: I used to think the light was for the ships. # thoughtful
    Isla: Then I realised I kept it on for me. So I'd know where home was.
    Isla: Now I know where home is. It's got a bell over the door. # happy
    ~ flag(isla_together)
    ~ friendship(15)
* {not romance} [Help her repaint the lamp room]
    Isla: Thanks for this. For all of it. # happy
    ~ friendship(10)
-> END
