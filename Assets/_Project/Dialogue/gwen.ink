// Gwen Tallow (she/her): the harbourmaster, fifties. Loud, capable, runs the harbour like a ship. Friendship only. Chapter 2.
// Knot names follow Docs/ADDING_CONTENT.md, "Dialogue".

=== gwen_intro ===
A woman in a yellow oilskin strides in and shakes the rain off like a dog. # happy
Gwen: Gwen Tallow. Harbourmaster. If it floats in this town, it's my business. # happy
~ met()
~ note(The harbourmaster: if it floats, it's her business.)
-> END

=== gwen_visit ===
Gwen: Tide's on the turn. I've got twenty minutes. Impress me. # happy
-> END

=== gwen_visit_ropes ===
Gwen: Some fool tied a bowline wrong on the east pontoon. I could weep. # sad
-> END

=== gwen_visit_2_storm ===
Gwen: Storm coming Thursday. Batten your shop down. I mean it. # thoughtful
~ note(Warns the whole town about storms.)
-> END

=== gwen_request_office ===
# fits: tablecloth, sampler, hoop_art
# best: sampler
Gwen: My office is all charts and coffee rings. # thoughtful
Gwen: Something on the wall that isn't a tide table. Something with a bit of heart.
-> END

=== gwen_request_bag ===
# fits: tote, scarf
# best: tote
Gwen: I carry my whole life in a carrier bag. It's undignified. # neutral
Gwen: Something sturdy. Salt-proof, ideally.
-> END

=== gwen_handover_good ===
Gwen: Now that's shipshape. Proper job. # happy
-> END

=== gwen_handover_okay ===
Gwen: That'll do me. Good work. # happy
-> END

=== gwen_handover_poor ===
Gwen: Bit rough round the edges. Like me. I'll take it. # thoughtful
-> END

=== gwen_suggest_yes ===
Gwen: Aye, make me one. # happy
-> END

=== gwen_suggest_buy ===
Gwen: I'll have the pattern. Long nights in the office. # happy
-> END

=== gwen_suggest_no ===
Gwen: Not for me, love. # neutral
-> END

=== gwen_scene_1 ===
Gwen: Your great-aunt sewed every flag on this harbour. Every single one. # thoughtful
* [I had no idea.]
    Gwen: Most people don't. That's how she liked it. # happy
    ~ note(Your great-aunt sewed every flag on the harbour.)
    ~ friendship(10)
* [Do any still fly?]
    Gwen: The blue one on the lifeboat house. Thirty years and it's still flying. # happy
    ~ friendship(10)
-> END

=== gwen_scene_2 ===
Gwen: I lost my husband to the sea. Twenty years ago. # sad
Gwen: I took this job so I'd be the one watching the water.
* [That's very brave.]
    Gwen: It's very stubborn. Same thing, sometimes. # thoughtful
    ~ friendship(10)
* [Sit with me a while.]
    You sit together on the harbour wall until the tide turns. # thoughtful
    ~ friendship(10)
-> END

=== gwen_scene_3 ===
Gwen: The harbour's getting new flags for the festival. # happy
Gwen: I want them from your shop. Win's shop. It's only right.
~ friendship(15)
-> END

=== gwen_saturday ===
Gwen takes you round the harbour in her launch, pointing out every boat and its owner's worst secret. # happy
Gwen: That one's never been out. Fifteen years. Polished every Sunday. # happy
-> END
