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
* [What did she make?]
    Tobias: Everything. But the tablecloth. With the sea round the edge. That one I'd give the boat for. # sad
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
Tobias: If it's anywhere, it's in the loft. I'm too old for ladders. Will you look?
* [Of course I will.]
    ~ flag(search_loft)
    ~ friendship(5)
    Tobias: Good. Bring a torch. And watch the third rung. # happy
-> END
