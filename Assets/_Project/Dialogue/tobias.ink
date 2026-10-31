// Old Tobias: a retired fisherman looking for his late wife Elsa's tablecloth pattern. Friendship only.

=== tobias_visit ===
Tobias: Mm. Afternoon. # neutral
Tobias: Your great-aunt kept the door on a bell like that. Same bell, I'd wager.
-> END

=== tobias_visit_2 ===
Tobias: There you are. Brought you mackerel. Don't make a face, it's fresh this morning. # happy
-> END

=== tobias_request_table ===
# fits: tablecloth, sampler, hoop_art
# best: tablecloth
Tobias: Sunday dinners have gone quiet since Elsa. # sad
~ note(His wife, Elsa, has passed away. Sunday dinners have gone quiet.)
Tobias: The table looks bare. Needs something on it. Something that remembers there used to be more of us round it.
-> END

=== tobias_request_wall ===
# fits: sampler, hoop_art, tablecloth
# best: sampler
Tobias: There's a pale square on the wall where the barometer used to hang. # thoughtful
Tobias: Barometer's broken. Square's still there. Want something to cover it that isn't a barometer.
-> END

=== tobias_handover_good ===
Tobias: ... # surprised
Tobias: Elsa would have liked that. That's the best thing I can say about anything. # happy
-> END

=== tobias_handover_okay ===
Tobias: Good, honest work. Thank you. # neutral
-> END

=== tobias_handover_poor ===
Tobias: It'll do the job. We all start somewhere, lad or lass. # thoughtful
-> END

=== tobias_scene_1 ===
Tobias: Elsa used to say I couldn't tell a cross stitch from a crossword. # thoughtful
Tobias: She'd sit by the window with her hoop while I mended nets. Both of us working with thread, she said. Different catches.
~ note(Was a fisherman. Elsa stitched by the window while he mended nets.)
* [What did she make?]
    Tobias: Everything. But the tablecloth. With the sea round the edge. That one I'd give the boat for. # sad
    ~ note(Elsa made a tablecloth with the sea round its edge. He'd give anything for it.)
    ~ friendship(5)
* [Different catches. I like that.]
    Tobias: Aye. Hers were prettier. # happy
    ~ friendship(5)
-> END

=== tobias_scene_2 ===
Tobias: Found this in the shed. # neutral
He puts a tin on the counter. Inside: a needle case, a thimble worn smooth, and a scrap of linen with half a wave stitched on it.
Tobias: It's hers. I want you to have it. You'll know what to do with it better than an old man.
* [I'll keep it safe.]
    ~ friendship(10)
    ~ flag(elsas_tin)
    Tobias: I know you will. # happy
-> END

=== tobias_scene_3 ===
Tobias: Your great-aunt's book. The missing pages. # thoughtful
Tobias: I think Elsa borrowed one. Forty years ago, maybe. Never gave it back, the rascal.
~ note(Thinks Elsa borrowed a page of your great-aunt's book. It may be in his loft.)
Tobias: If it's anywhere, it's in the loft. I'm too old for ladders. Will you look?
* [Of course I will.]
    ~ flag(search_loft)
    ~ friendship(5)
    Tobias: Good. Bring a torch. And watch the third rung. # happy
-> END

// ---- More lines (2026-10-30)

=== tobias_visit_weather ===
Tobias: Wind's from the north. Fish'll be deep today. # neutral
Tobias: Not that it matters to me anymore. Habit.
-> END

=== tobias_visit_kettle ===
Tobias: Your great-aunt kept the kettle on the back shelf. # thoughtful
Tobias: I'm not asking for tea. I'm just saying where she kept it.
-> END

=== tobias_visit_1_boat ===
Tobias: Sold the boat this morning. Forty years, that boat. # sad
~ note(Sold his boat after forty years.)
Tobias: The lad who bought it will paint it blue. Elsa would've liked blue.
-> END

=== tobias_visit_2_photo ===
Tobias: Found this in a drawer. # thoughtful
He shows you a photograph: a young woman laughing at a table, a cloth of anchors under her elbows.
Tobias: That's the one. That's the cloth.
~ note(The tablecloth had anchors on it.)
-> END

=== tobias_visit_3_soup ===
Tobias: Brought you soup. Fish soup, before you ask. # happy
Tobias: You look like someone who forgets lunch.
-> END

=== tobias_request_grandson ===
# fits: sweater, scarf
# best: sweater
Tobias: My grandson's starting on the trawlers. Seventeen. Thinks he's made of iron. # thoughtful
~ note(His grandson, seventeen, is starting on the trawlers.)
Tobias: He's not. Nobody is, out there at five in the morning.
-> END

=== tobias_handover_good_quiet ===
Tobias: ... # thoughtful
Tobias: Aye. That's right. That's how it should be. # happy
-> END

=== tobias_handover_okay_fair ===
Tobias: Fair work. Honest work. Can't ask more than that. # neutral
-> END

=== tobias_handover_poor_gruff ===
Tobias: Hm. You'll get there. Took me ten years to tie a decent knot. # neutral
-> END

// ---- Reactions when you suggest a pattern on display: _yes (they want you to make it), _buy (they make
// that craft themselves), _no. Add variants like name_suggest_no_rain. ----

=== tobias_suggest_yes ===
Tobias: Hm. Aye. That's the kind of thing Elsa would have had on the table. # thoughtful
-> END

=== tobias_suggest_no ===
Tobias: Pretty. Not for an old fisherman, though. What would I do with it? # neutral
-> END

=== tobias_suggest_no_fuss ===
Tobias: No. Too much fuss for me. You keep it for someone who'll love it. # neutral
-> END
