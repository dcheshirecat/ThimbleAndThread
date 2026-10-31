// Isla Marlowe: keeper of the old lighthouse. Adult; romanceable. Arrives in chapter 2.
// Dry, gruff, secretly hopeless at knitting. Friendship scenes at levels 1-3; at level 3 the player can
// ask her out (~ romance(1)), and scenes 4 and 5 follow the romance if they did.

=== isla_visit ===
Isla: You're the new one. The great-{niece}. # neutral
Isla: I'm Isla. The lighthouse. Well, not the lighthouse. I keep it.
~ note(Keeps the lighthouse.)
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
~ note(Has been trying to knit a sock for three winters.)
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
~ note(The lighthouse has a hundred and twelve steps.)
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

// ---- More lines (2026-10-30)

=== isla_visit_fog ===
Isla: Fog's in. Had the horn going all night. I can still hear it. # thoughtful
Isla: Talk to me about something that isn't fog.
-> END

=== isla_visit_gull ===
Isla: A gull stole my sandwich. Out of my hand. Looked me in the eye while it did it. # neutral
-> END

=== isla_visit_1_wool ===
Isla: I've been practising. Don't look so surprised. # neutral
Isla: I have made one row that I'm not ashamed of. One.
-> END

=== isla_visit_2_stars ===
Isla: Clear night tonight. You can see the whole Plough from the gallery. # thoughtful
Isla: Just saying. If anyone wanted to see it.
-> END

=== isla_visit_4_morning ===
Isla: Morning. I brought you a coffee. And then I drank half of it on the way. # happy
Isla: The half that's left is for you.
-> END

=== isla_request_logbook ===
# fits: tablecloth, sampler, hoop_art
# best: tablecloth
Isla: The keepers' table hasn't had a cloth on it since my grandad's time. # thoughtful
~ note(Her grandad kept the lighthouse before her.)
Isla: Feels like it should. Something sturdy. Something that's seen a few storms.
-> END

=== isla_handover_good_quiet ===
Isla: I'm going to say thank you, and you're not going to make a thing of it. # happy
Isla: Thank you.
-> END

=== isla_handover_okay_nod ===
Isla: Good. Solid. Like a decent rope. # neutral
-> END

=== isla_handover_poor_dry ===
Isla: I've seen worse. I've made worse. Frequently. # thoughtful
-> END

// ---- Reactions when you suggest a pattern on display: _yes (they want you to make it), _buy (they make
// that craft themselves), _no. Add variants like name_suggest_no_rain. ----

=== isla_suggest_yes ===
Isla: That'd stand up to the wind. Yes. Make me one. # neutral
-> END

=== isla_suggest_buy ===
Isla: I'll take the pattern. I'm going to beat this knitting thing if it takes me ten more winters. # thoughtful
-> END

=== isla_suggest_no ===
Isla: Too delicate for the tower. It'd be in the sea by Tuesday. # neutral
-> END

// ---- First meeting (owner, 2026-10-31): they introduce themselves. ~ met() puts their name and portrait
// in the Journal (until then they show as ???). Played once, instead of the usual hello.

=== isla_intro ===
A woman in a salt-stiff coat looks the shop over like she's checking it for leaks. # neutral
Isla: You're the new one. The great-{niece}. # neutral
Isla: I'm Isla. The lighthouse. Well, not the lighthouse. I keep it.
~ met()
~ note(Keeps the lighthouse.)
Isla: Your aunt mended my jumpers. I'm told I'm hard on jumpers. # thoughtful
-> END
