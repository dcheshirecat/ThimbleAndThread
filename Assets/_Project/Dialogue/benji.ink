// Benji Marsh (he/him): a nervous young carpenter getting married in the spring. Kind, anxious, over-prepares. Friendship only. Chapter 3.
// Knot names follow Docs/ADDING_CONTENT.md, "Dialogue".

=== benji_intro ===
A young man with sawdust in his hair hovers in the doorway as if the shop might bite. # surprised
Benji: Hi. Hello. I'm Benji. Marsh. I'm getting married. Sorry. That's not my name, that's just... news. # happy
~ met()
~ note(A carpenter, getting married in the spring.)
-> END

=== benji_visit ===
Benji: I made a list of questions and then lost the list. # sad
-> END

=== benji_visit_seating ===
Benji: The seating plan has broken me. Two aunts. One table. War. # sad
-> END

=== benji_visit_2_sam ===
Benji: Sam says I worry too much. Sam's right. I'm worried about that too. # happy
~ note(He's marrying Sam.)
-> END

=== benji_request_wedding ===
# fits: hoop_art, sampler, tablecloth
# best: hoop_art
Benji: I want to give Sam something on the morning of the wedding. # thoughtful
Benji: Something that'll still be on our wall when we're old.
-> END

=== benji_request_table ===
# fits: tablecloth, tote
# best: tablecloth
Benji: I built us a dining table. Out of the old boatyard's oak. # happy
Benji: It deserves something special on it. For the first dinner.
-> END

=== benji_handover_good ===
Benji: Oh no, I'm going to cry. I'm crying. It's perfect. # happy
-> END

=== benji_handover_okay ===
Benji: That's lovely. Really, really lovely. # happy
-> END

=== benji_handover_poor ===
Benji: It's... got a lot of love in it. I can tell. # thoughtful
-> END

=== benji_suggest_yes ===
Benji: Yes! Please! Before I change my mind about everything! # happy
-> END

=== benji_suggest_buy ===
Benji: Sam stitches. Could I take the pattern for Sam? # happy
-> END

=== benji_suggest_no ===
Benji: I don't think it's quite... us. Sorry! # thoughtful
-> END

=== benji_scene_1 ===
Benji: Can I ask you something? Did your great-aunt ever marry? # thoughtful
* [I don't think so.]
    Benji: I hope she was happy. You can be happy all sorts of ways. # happy
    ~ friendship(10)
* [I think she married this shop.]
    Benji: Ha! I'm marrying a woodshop, basically. Sam knew what they were getting. # happy
    ~ friendship(10)
-> END

=== benji_scene_2 ===
Benji: I built you something. It's a thread rack. It might be wonky. # happy
It isn't wonky. It's beautiful, oak, and has exactly one hundred spools.
* [Benji, this is incredible.]
    Benji: It's nothing. It's not nothing. I'm glad you like it. # happy
    ~ friendship(10)
* [How long did this take?]
    Benji: Don't tell Sam. Three weekends. # happy
    ~ friendship(10)
-> END

=== benji_scene_3 ===
Benji: Will you come to the wedding? Sam says you have to. I say you have to. # happy
~ flag(benji_wedding)
~ friendship(15)
-> END

=== benji_saturday ===
Benji shows you round his workshop: shavings everywhere, a radio, a half-built cradle he won't explain. # happy
Benji: That's for... later. Much later. Don't tell Sam. # surprised
-> END

// ---- In his workshop (Marsh's Workshop, the furniture shop) ----

=== benji_workshop ===
A workshop that smells of pine shavings and varnish. A young man looks up from a half-finished chair and nearly drops his plane. # surprised
Benji: Oh! A customer. A real one. Hi. I'm Benji. Marsh. This is my workshop. It's also my dad's, but he's mostly retired. # happy
~ met()
~ note(A carpenter with a workshop down the lane. He makes furniture.)
Benji: Everything's made here. If it wobbles, bring it back. It won't wobble. Probably. # thoughtful
-> END

=== benji_greet ===
Benji: Hi! Mind the shavings. Have a look round. # happy
-> END

=== benji_greet_sawdust ===
Benji: Sorry about the noise. I'm sanding a cradle. For a friend. Not for me. Not yet. # thoughtful
-> END

=== benji_bye ===
Benji: See you! Tell people about the chairs. If they ask. # happy
-> END

=== benji_thanks ===
Benji: Thank you! I'll carry it over myself. Well, my dad will. I'll supervise. # happy
-> END

// ---- More lines (2026-11-02: so nobody repeats themselves), and how they take a present ----
=== benji_visit_measure ===
Benji: I measured the church doorway three times this morning. In case the cake doesn't fit. # thoughtful
Benji: Sam says cakes don't need measuring. Sam is very calm. I love that about them. I don't understand it, but I love it.
-> END
=== benji_visit_splinter ===
Benji: Don't mind the plaster. Occupational hazard. Twenty years and I still forget which end of the chisel's sharp. # happy
-> END
=== benji_visit_3_vows ===
Benji: I've written my vows. Nine drafts. The ninth one's just "I'll build you anything." # thoughtful
Benji: Is that enough? It feels like it might be enough.
-> END
=== benji_saturday_bench ===
Benji shows you the bench he's building for the church porch, so people can sit and wait for weddings. You sand the arms smooth together. # happy
Benji: Every bench is a promise someone'll have somewhere to rest. That's what my dad said, anyway. # thoughtful
-> END
=== benji_gift ===
Benji: For me? Oh, I'm going to put this somewhere nobody can get sawdust on it. Which is nowhere. But I'll try. # happy
-> END
