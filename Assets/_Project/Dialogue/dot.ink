// Dot Pascoe (she/her): eighty-one, runs the tea room above the bakery. Sharp as a pin, knows everyone's business,
// and was your great-aunt's oldest friend. Friendship only. Arrives in chapter 3.

=== dot_intro ===
A tiny woman with a walking stick and a very large handbag looks you up and down. # thoughtful
Dot: So you're the one. Taller than she said. Win always said you'd be tall. # happy
Dot: Dorothy Pascoe. Dot. I run the tea room. I've known this shop longer than you've been alive. # happy
~ met()
~ note(Runs the tea room above the bakery; your great-aunt's oldest friend.)
-> END

=== dot_visit ===
Dot: Don't mind me, I'm only looking. I'm always only looking. # happy
-> END

=== dot_visit_scones ===
Dot: I've brought you a scone. Don't argue. Win never argued about scones. # happy
~ note(Brings scones; won't take no for an answer.)
-> END

=== dot_visit_hip ===
Dot: New hip. Titanium. I'm practically a robot now. # happy
-> END

=== dot_visit_2_win ===
Dot: Win used to sit in that very window and judge people's knitting as they walked past. # happy
Dot: Out loud. Through the glass. I miss her terribly.
-> END

=== dot_request_tearoom ===
# fits: tablecloth, sampler
# best: tablecloth
Dot: My tea room cloths are older than the tea room. They've seen things. # thoughtful
Dot: Something fresh, for the window table. The good table.
-> END

=== dot_request_grandson ===
# fits: plushie, plushie_sewn, sweater
# best: plushie
Dot: My great-grandson's coming to stay. He's three and he bites. # happy
Dot: Something cuddly that can survive being bitten.
-> END

=== dot_request_hoop ===
# fits: hoop_art, sampler
# best: hoop_art
Dot: My wall has a nail and nothing on it. Fifty years. # thoughtful
Dot: Something Win would have liked. You'll know what that means better than I do.
-> END

=== dot_handover_good ===
Dot: Oh, my. Win would be so proud. I'm going to have to sit down. # happy
-> END

=== dot_handover_okay ===
Dot: Very nice, dear. Very nice indeed. # happy
-> END

=== dot_handover_poor ===
Dot: Practice, practice. Win's first ones were dreadful too. Shocking. # thoughtful
-> END

=== dot_suggest_yes ===
Dot: I'll take one. I'll take two if you're quick. # happy
-> END

=== dot_suggest_buy ===
Dot: I'll stitch that myself, thank you. These hands still work. # happy
-> END

=== dot_suggest_no ===
Dot: Not for me, dear. Too modern. Or too old. One of those. # thoughtful
-> END

=== dot_scene_1 ===
Dot: Sit with me a minute. # thoughtful
Dot: Win and I met in this shop. 1962. She was buying red thread, and I was stealing it.
* [You were stealing it?]
    Dot: Badly. She caught me and made me help her stitch a whole tablecloth as punishment. # happy
    Dot: Sixty years of friendship over a reel of red thread.
    ~ note(Met your great-aunt in 1962 by trying to steal red thread.)
    ~ friendship(10)
* [What was she like, back then?]
    Dot: Impossible. Wonderful. Mostly impossible. # happy
    ~ friendship(5)
-> END

=== dot_scene_2 ===
Dot: I want you to have this. # thoughtful
A thimble, silver, worn smooth on one side.
Dot: It was Win's. She gave it to me when my hands got bad. They're better now. It should be back in the shop.
* [I'll use it every day.]
    Dot: Good. Things are for using. # happy
    ~ friendship(10)
* [Are you sure?]
    Dot: I'm eighty-one. I'm always sure. # happy
    ~ friendship(10)
-> END

=== dot_scene_3 ===
Dot: You know the torn pages from her book? # thoughtful
Dot: She tore them out herself. Gave them away. To the people who mattered.
Dot: Ask around. You'd be surprised who's been keeping a page safe.
~ friendship(15)
-> END

=== dot_saturday ===
Dot teaches you her scone recipe in the tea room kitchen. The secret, she says, is cold hands and a warm heart. # happy
Dot: And a ridiculous amount of butter. Don't tell the doctor. # happy
-> END

// ---- More lines (2026-11-02: so nobody repeats themselves), and how they take a present ----
=== dot_visit_gossip ===
Dot: The new vicar's bees got into the bakery on Tuesday. Nobody's admitted it, but the custard slices were never the same. # thoughtful
-> END
=== dot_visit_dance ===
Dot: I went dancing at the Legion on Friday. Eighty-one and I can still do the cha-cha. Badly. # happy
-> END
=== dot_visit_3_photo ===
Dot: I found a photo of me and Win at the 1966 regatta. Matching dresses. We made them ourselves, the night before. # thoughtful
Dot: She sewed hers better. She always did. Don't tell her I said that, wherever she is.
-> END
=== dot_saturday_tea ===
Dot teaches you her scone recipe, which is mostly butter and a stern talking-to. You burn the first batch. She eats one anyway. # happy
Dot: Character. That's what that is. # happy
-> END
=== dot_gift ===
Dot: Oh, my dear. Win would be so proud of you. I'm going to put this right where she used to sit. # happy
-> END
