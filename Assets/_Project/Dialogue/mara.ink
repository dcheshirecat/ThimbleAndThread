// Mara: a nervous new parent who loves anything soft. Friendship only.
// Knot names the game looks for (see Docs/ADDING_CONTENT.md, "Dialogue"):
//   mara_visit, mara_visit_N (greeting at friendship level N or more), mara_request_*, mara_handover_good/okay/poor,
//   mara_scene_N (a scene played once, when friendship reaches level N).

=== mara_visit ===
Mara: Oh! Hello. Sorry, I always feel like I'm interrupting. # thoughtful
Mara: She finally fell asleep in the pram, so I've got... about four minutes.
~ note(Has a baby daughter, and almost no sleep.)
-> END

=== mara_visit_2 ===
Mara: It's you! I mean, of course it's you, it's your shop. # happy
Mara: She smiled at the bunny you made. Actually smiled. I cried a bit.
-> END

=== mara_request_cold ===
# fits: scarf, sweater, plushie_sewn, plushie
# best: scarf
Mara: The nights have turned. She will NOT keep a blanket on, she kicks it off like it's offended her. # thoughtful
Mara: I want something that keeps her warm but doesn't come off the moment I look away.
-> END

=== mara_request_soft ===
# fits: plushie_sewn, plushie, tote
# best: plushie_sewn
Mara: Everything in the baby shops is plastic and beeps. # sad
Mara: I'd love something soft. Something she can hold onto when I'm not there.
-> END

=== mara_handover_good ===
Mara: Oh. Oh, it's perfect. # happy
Mara: You've no idea how much I needed something to go right today.
-> END

=== mara_handover_okay ===
Mara: It's lovely, thank you. She'll put it straight in her mouth, so... that's a compliment, really. # happy
-> END

=== mara_handover_poor ===
Mara: That's... thank you. No, really. It's the thought, isn't it? # thoughtful
-> END

=== mara_scene_1 ===
Mara: Can I sit for a minute? Just a minute. # thoughtful
Mara: Everyone tells me to sleep when the baby sleeps. When am I meant to do the washing? Sleep when the washing sleeps?
* [Laugh] You laugh, and so does she, properly.
    Mara: Sorry. I haven't talked to a grown-up in three days. # happy
    ~ friendship(5)
* [Put the kettle on]
    Mara: You're an angel. Two sugars. Don't judge me. # happy
    ~ friendship(5)
- Mara: Your great-aunt used to give me humbugs when I was little. This shop always felt safe.
~ note(Grew up coming into this shop. Your great-aunt gave her humbugs.)
-> END

=== mara_scene_2 ===
Mara: I made something. Don't laugh. # thoughtful
She holds up a lumpy square of knitting, more hole than wool.
* [It's wonderful.]
    Mara: Liar. Lovely liar. # happy
* [Want me to show you how to fix it?]
    Mara: Would you? I want to make her something myself. Just once. # happy
    ~ note(Is learning to knit, to make her daughter something herself.)
    ~ friendship(5)
- Mara: Maybe next time it'll have fewer holes. Maybe.
-> END

=== mara_scene_3 ===
Mara: Her first word was "dog". Not "mama". Dog. We don't even HAVE a dog. # surprised
Mara: But I'm putting it in her baby book anyway, and I want the page to have a little stitched dog on it. You'll help?
* [Of course.]
    ~ friendship(10)
    Mara: You're my favourite person in this whole town. Don't tell anyone. # happy
-> END

// ---- More lines (2026-10-30). Variants: any mara_visit..., mara_handover_good... knot can be picked;
// the game prefers ones not heard lately. A number right after _visit_ is the friendship level needed.

=== mara_visit_rain ===
Mara: Is it still raining? It's always raining. I've forgotten what my hair looks like dry. # thoughtful
-> END

=== mara_visit_list ===
Mara: I made a list of what I came in for. # neutral
Mara: I've left the list at home. On the list, probably, is "remember the list". # sad
-> END

=== mara_visit_1_nap ===
Mara: Two whole hours last night. In a row! I feel like a new person. A slightly damp new person. # happy
-> END

=== mara_visit_2_word ===
Mara: She said her first word yesterday. # happy
Mara: It was "duck". We don't have a duck. We've never had a duck. I'm choosing not to worry about it.
-> END

=== mara_visit_3_friend ===
Mara: You know you're the only grown-up I talk to some days? Apart from the postman. # thoughtful
~ note(Some days you're the only grown-up she talks to.)
Mara: And he only says "sign here".
-> END

=== mara_request_bath ===
# fits: sampler, hoop_art, tablecloth
# best: sampler
Mara: The nursery walls are so bare. The books all say "a calm, stimulating space". # thoughtful
Mara: I don't know how a room can be both. Something gentle to look at, maybe?
-> END

=== mara_handover_good_hug ===
Mara: Can I hug you? I'm going to hug you. Sorry. Thank you. # happy
-> END

=== mara_handover_okay_fine ===
Mara: That'll do beautifully. Honestly, anything that isn't covered in sick is a win this week. # happy
-> END

=== mara_handover_poor_kind ===
Mara: Oh! It's... very handmade. That's the point, isn't it? # thoughtful
-> END

// ---- Reactions when you suggest a pattern on display: _yes (they want you to make it), _buy (they make
// that craft themselves), _no. Add variants like name_suggest_no_rain. ----

=== mara_suggest_yes ===
Mara: Oh, she'd love that. I'd love that. Could you, really? # happy
-> END

=== mara_suggest_buy ===
Mara: I've been practising, you know! I'd love to try that one myself. Slowly. With a lot of swearing. # happy
-> END

=== mara_suggest_no ===
Mara: It's gorgeous. It's just not... baby-proof. Nothing is, right now. # thoughtful
-> END

// ---- First meeting (owner, 2026-10-31): they introduce themselves. ~ met() puts their name and portrait
// in the Journal (until then they show as ???). Played once, instead of the usual hello.

=== mara_intro ===
A young woman edges in sideways, a pram first, apologising to the doorframe. # thoughtful
Mara: Hi. Sorry. Hello. I'm Mara. # thoughtful
~ met()
Mara: We live above the bakery. This is Poppy. She's asleep, which is a miracle, so I'm whispering.
~ note(Has a baby daughter, Poppy, and almost no sleep.)
Mara: I heard the shop had opened again. I used to come in with my mum when I was little. # happy
-> END

// ---- Saturdays (owner, 2026-10-31): the shop is closed; friends at level 2+ can be visited (name_saturday*),
// and someone you're seeing takes you on a date (name_date*). Add variants freely.

=== mara_saturday ===
Mara answers the door with Poppy on her hip and a muslin over her shoulder. # happy
Mara: You came! Sit anywhere that isn't sticky. So, the floor. # happy
You spend the afternoon on a picnic blanket in the back garden, Poppy grabbing at the grass.
Mara: This is the first Saturday in months I haven't felt like a zombie. Thank you. # happy
-> END

=== mara_saturday_park ===
You push the pram round the harbour park while Mara finally drinks a hot coffee, start to finish. # happy
Mara: Look at that. It's still hot. I'd forgotten coffee was supposed to be hot. # surprised
-> END
