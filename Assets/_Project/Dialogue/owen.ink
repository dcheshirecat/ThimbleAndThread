// The Reverend Owen Price (he/him): the vicar at St. Brigid's, seventies, gentle, terrible jokes, beekeeper. Friendship only. Chapter 3.
// Knot names follow Docs/ADDING_CONTENT.md, "Dialogue".

=== owen_intro ===
An old man in a dog collar and a cardigan holds the door for nobody in particular. # happy
Owen: Owen Price. I'm the vicar up at St. Brigid's. Don't worry, I'm off duty. Mostly. # happy
~ met()
~ note(The vicar at St. Brigid's.)
-> END

=== owen_visit ===
Owen: Why did the vicar cross the road? To get to the other side of the sermon. I'm sorry. # happy
-> END

=== owen_visit_bees ===
Owen: The bees were very cross with me this morning. I'd forgotten their birthday. # happy
~ note(Keeps bees behind the church.)
-> END

=== owen_visit_2_win ===
Owen: Your great-aunt sang in the choir for forty years. Flat as a pancake. We loved her. # happy
-> END

=== owen_request_altar ===
# fits: tablecloth, sampler
# best: tablecloth
Owen: The altar cloth has a hole the shape of Wales. # thoughtful
Owen: Something new, for Easter. Something that would make the congregation look up.
-> END

=== owen_request_cold ===
# fits: scarf, sweater
# best: scarf
Owen: The vestry is the coldest room in the county. Possibly the country. # sad
Owen: Something warm for Sunday mornings. Something cheerful under the robes.
-> END

=== owen_handover_good ===
Owen: Oh, that's a blessing. I mean that in every sense. # happy
-> END

=== owen_handover_okay ===
Owen: Very fine. Very fine indeed. # happy
-> END

=== owen_handover_poor ===
Owen: It'll be loved. That's what matters. # happy
-> END

=== owen_suggest_yes ===
Owen: Yes, please. The Lord loves a quick decision. # happy
-> END

=== owen_suggest_buy ===
Owen: The Mothers' Union would love that pattern. May I? # happy
-> END

=== owen_suggest_no ===
Owen: Not quite right for the church, I'm afraid. # thoughtful
-> END

=== owen_scene_1 ===
Owen: Your great-aunt made the kneelers in the church. All sixty of them. # thoughtful
* [I'd love to see them.]
    Each one different: boats, gulls, a lighthouse, a cat that looks suspiciously like Midge's. # happy
    ~ note(Your great-aunt stitched all sixty church kneelers.)
    ~ friendship(10)
* [Sixty! How long did that take?]
    Owen: Twelve years. She did one every Lent. # happy
    ~ friendship(10)
-> END

=== owen_scene_2 ===
Owen: I'm retiring next year. I don't quite know who I'll be when I'm not the vicar. # thoughtful
* [A beekeeper with very bad jokes.]
    Owen: Ha! Yes. That's a fine thing to be. # happy
    ~ friendship(10)
* [The same person. Just with more Sundays.]
    Owen: That's kind. That's very kind. # happy
    ~ friendship(10)
-> END

=== owen_scene_3 ===
Owen: The bees made a record harvest. I've brought you a jar. # happy
Owen: Don't tell the Mothers' Union, they each only got half a jar.
~ friendship(15)
-> END

=== owen_saturday ===
Owen shows you his beehives behind the church. You're given a veil and absolutely no instructions. # happy
Owen: They know you're a friend. Bees always know. # happy
-> END

// ---- More lines (2026-11-02: so nobody repeats themselves), and how they take a present ----
=== owen_visit_joke ===
Owen: Why did the bee get married? Because he found his honey. # happy
Owen: I'll see myself out. No, I won't, I've come to buy thread.
-> END
=== owen_visit_bells ===
Owen: The bell-ringers want new bell-rope sallies. The fluffy bits. Could anyone knit those, do you think? # thoughtful
-> END
=== owen_visit_3_doubt ===
Owen: People think vicars have all the answers. # thoughtful
Owen: Mostly I have tea and a willingness to sit with people. It goes further than you'd think.
-> END
=== owen_saturday_honey ===
You help Owen spin the honey out of the frames in the vestry. Everything is sticky for the rest of the day, including the hymn books. # happy
Owen: The Lord provides. Also the bees. Mainly the bees. # happy
-> END
=== owen_gift ===
Owen: For me? That's... Bless you. Truly. I'll keep it in the vestry, where I can see it on long Sundays. # happy
-> END
