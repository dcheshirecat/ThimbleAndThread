// Rowan Hale (they/them): owner of Knot & Needle, the rival shop across the square. Adult; romanceable.
// Arrives in chapter 4. Competitive, charming, secretly admires the great-aunt's shop.

=== rowan_visit ===
Rowan: Checking out the competition. Purely professional. # happy
Rowan: Your window display's better than mine. I hate that.
~ note(Runs Knot & Needle, the other craft shop in town.)
-> END

=== rowan_visit_3 ===
Rowan: I closed early. Don't tell my customers. Actually, tell them. Send them here, I don't mind. # happy
-> END

=== rowan_request_mascot ===
# fits: plushie, plushie_sewn
# best: plushie
Rowan: Every good shop needs a mascot. Mine has a cardboard cutout of a sheep. # thoughtful
Rowan: It's undignified. Make me something with a face that people want to hug.
-> END

=== rowan_request_bags ===
# fits: tote, hoop_art, sampler
# best: tote
Rowan: Paper bags are killing me. They tear, they blow away, they end up in the harbour. # sad
Rowan: I want something better. Something people keep.
-> END

=== rowan_handover_good ===
Rowan: Okay. That is genuinely annoying. It's perfect. # surprised
Rowan: I'm going to tell everyone I made it. I'm kidding. Mostly. # happy
-> END

=== rowan_handover_okay ===
Rowan: Solid. I'd sell that. Well. You sold it. To me. # happy
-> END

=== rowan_handover_poor ===
Rowan: Rough day? Mine too. Let's call it rustic. # thoughtful
-> END

=== rowan_scene_1 ===
Rowan: Can I tell you a secret? # thoughtful
Rowan: I learned to crochet in this shop. Your great-aunt taught me. I was nine and furious about everything.
~ note(Learned to crochet here, from your great-aunt, at nine years old.)
Rowan: She gave me a hook and said "furious is fine, just be furious in rows".
* [That sounds like her.]
    Rowan: You didn't even meet her properly, did you? I'm sorry. # sad
    ~ friendship(5)
* [So why open a rival shop?]
    Rowan: Rival is a strong word. Neighbour. Competitive neighbour. # happy
    ~ friendship(5)
-> END

=== rowan_scene_2 ===
Rowan: Truce for an evening? I have biscuits and an unreasonable amount of yarn. # happy
You spend the evening in Knot & Needle's back room, untangling a skein the size of a sheep.
Rowan: This is the most fun I've had since I opened. Don't tell anyone that either.
* [Same time next week?]
    Rowan: It's a date. I mean. A standing appointment. A... yes. # surprised
    ~ friendship(10)
* [Your secrets are safe with me.]
    Rowan: I'm counting on it. # happy
    ~ friendship(5)
-> END

=== rowan_scene_3 ===
Rowan: The festival committee wants one shop to run the craft tent. # thoughtful
Rowan: I was going to fight you for it. I don't want to fight you for it. I want to run it with you.
* [Together, then. And maybe dinner after?]
    Rowan: ... Yes. Dinner. With you. Yes. # happy
    ~ romance(1)
    ~ friendship(10)
* [Together, then. Partners in thread.]
    Rowan: Partners in thread. I like that. # happy
    ~ friendship(5)
-> END

=== rowan_scene_4 ===
* {romance} [Dinner at the harbour cafe]
    Rowan: I practised what to say in the mirror. Then I forgot all of it. # thoughtful
    Rowan: So I'll just say: you make this town feel less like somewhere I ended up and more like somewhere I chose.
    ~ friendship(10)
* {not romance} [Plan the craft tent]
    Rowan: We're going to have the best tent at the festival. Obviously. # happy
    ~ friendship(5)
-> END

=== rowan_scene_5 ===
* {romance} [Close both shops early]
    Rowan: I had an idea. Don't laugh. # thoughtful
    Rowan: What if Knot & Needle didn't need its own door? What if it was the room at the back of yours?
    Rowan: Not a merger. A... thread. Between us. # happy
    ~ flag(rowan_together)
    ~ friendship(15)
* {not romance} [Toast the end of the rivalry]
    Rowan: To competitive neighbours. The best kind. # happy
    ~ friendship(10)
-> END

// ---- More lines (2026-10-30)

=== rowan_visit_spy ===
Rowan: Just checking out the competition. Professionally. # neutral
Rowan: Your window display's better than mine. I hate it. Congratulations.
-> END

=== rowan_visit_coffee ===
Rowan: I went to the café and ordered two by accident. Muscle memory. # thoughtful
Rowan: Here. Don't read into it.
-> END

=== rowan_visit_1_market ===
Rowan: Did you see the wool at the market this week? Daylight robbery. # neutral
Rowan: I bought three. Don't judge me.
-> END

=== rowan_visit_2_lesson ===
Rowan: I learned to crochet in this exact spot, you know. Your great-aunt made me redo my first chain forty times. # thoughtful
~ note(Learned to crochet here, from your great-aunt, at nine years old.)
Rowan: I still count my chains under my breath because of her.
-> END

=== rowan_visit_4_window ===
Rowan: I put one of your pieces in my window. With a sign saying where it's from. # happy
Rowan: Purely as a public service.
-> END

=== rowan_request_rivalry ===
# fits: sampler, hoop_art, tablecloth
# best: hoop_art
Rowan: I'm doing a "local makers" wall at Knot & Needle. It's very generous of me. # neutral
Rowan: It needs something from you, or people will ask why it isn't there.
-> END

=== rowan_handover_good_grudging ===
Rowan: Okay. Fine. That's better than anything I've made this year. # surprised
Rowan: Tell no one I said that. # happy
-> END

=== rowan_handover_okay_shrug ===
Rowan: Neat work. Bit safe. But neat. # neutral
-> END

=== rowan_handover_poor_tease ===
Rowan: Oh, this is going on my "inspiration" wall. Under "what not to do". I'm kidding! Mostly. # happy
-> END

// ---- Reactions when you suggest a pattern on display: _yes (they want you to make it), _buy (they make
// that craft themselves), _no. Add variants like name_suggest_no_rain. ----

=== rowan_suggest_yes ===
Rowan: I hate how much I want that. Fine. Make me one. # happy
-> END

=== rowan_suggest_buy ===
Rowan: Ooh, I'll make that myself. Professional curiosity. Don't tell my customers I bought a pattern from you. # happy
-> END

=== rowan_suggest_no ===
Rowan: It's good. It's just not me. I'll know it when I see it. # thoughtful
-> END

// ---- First meeting (owner, 2026-10-31): they introduce themselves. ~ met() puts their name and portrait
// in the Journal (until then they show as ???). Played once, instead of the usual hello.

=== rowan_intro ===
Someone in a beautifully cut coat lingers by your window display, then strolls in as if they own the place. # happy
Rowan: Rowan Hale. Knot & Needle, across the square. Pleasure. # happy
~ met()
~ note(Runs Knot & Needle, the other craft shop in town.)
Rowan: Checking out the competition. Purely professional.
Rowan: Your window display's better than mine. I hate that. # thoughtful
-> END

// ---- Saturdays (owner, 2026-10-31): the shop is closed; friends at level 2+ can be visited (name_saturday*),
// and someone you're seeing takes you on a date (name_date*). Add variants freely.

=== rowan_saturday ===
Rowan closes the Knot & Needle for an hour and you swap techniques over cold tea. # thoughtful
Rowan: Don't tell anyone we're friends. It's bad for the rivalry. # happy
-> END

=== rowan_date ===
Rowan takes you to the open-air cinema on the beach. You share a blanket they crocheted. # happy
Rowan: I made the blanket two-person-sized. That was... on purpose. # thoughtful
-> END
