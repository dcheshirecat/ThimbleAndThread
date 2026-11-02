// Jasper Quill (he/him): runs Quill's Curiosities, an antiques and oddments shop in the old customs house. Adult;
// romanceable. Opens once you reach level 6. Dry, kind, collects stories along with the objects.
// Chat with him in his shop (once a day); buying from him counts too. The player's name and pronouns: {name}, {they}...

=== jasper_intro ===
A tall man in a cardigan with mismatched elbow patches looks up from polishing a ship's compass. # thoughtful
Jasper: Ah. The haberdasher. Word travels fast in a town with one road. # happy
Jasper: Jasper Quill. Curiosities, oddments, and things with a past. Some of it even works.
~ met()
~ note(Runs Quill's Curiosities in the old customs house.)
Jasper: Everything in here does something for a room. Not just looks. You'll see. # thoughtful
-> END

=== jasper_visit ===
Jasper: Every object has a story. That clock stopped at the exact minute its owner proposed. Or so I tell buyers. # happy
-> END

=== jasper_visit_tea ===
Jasper: Tea? It's in a cup older than the town hall. Don't drop it. Actually, drop it. I'm tired of dusting it. # happy
~ note(Drinks tea from an antique cup he's tired of dusting.)
-> END

=== jasper_visit_2_aunt ===
Jasper: Your great-aunt sold me a sewing box once. Then bought it back a week later. Said she missed it. # thoughtful
-> END

=== jasper_scene_1 ===
Jasper shows you a drawer full of keys with no locks. # thoughtful
Jasper: I can't throw them away. Somewhere, a door's waiting.
~ note(Keeps a drawer of keys with no locks.)
* [That's lovely.]
    Jasper: Most people say "that's hoarding". # happy
    ~ friendship(5)
* [Want help finding the doors?]
    Jasper: ... Nobody's ever offered. # surprised
    ~ friendship(10)
-> END

=== jasper_scene_2 ===
Jasper: I was going to be a sailor. Got as far as the harbour and found a shop of old charts instead. # thoughtful
~ note(Meant to be a sailor; found a shop of old charts instead.)
~ friendship(5)
-> END

=== jasper_scene_3 ===
Jasper: There's a concert at the bandstand on Saturday. I have two tickets and, apparently, nerve. # thoughtful
* [I'd love to go with you.]
    Jasper: Oh. Good. Excellent. I'll wear the less patched cardigan. # happy
    ~ romance(1)
* [As friends? I'd like that.]
    Jasper: Friends who enjoy brass bands. The rarest kind. # happy
    ~ friendship(5)
-> END

=== jasper_greet ===
Jasper: Come in, come in. Mind the stuffed owl; he bites. # happy
-> END

=== jasper_greet_2_friend ===
Jasper: {name}! I put something aside I think you'll like. # happy
-> END

=== jasper_bye ===
Jasper: Just browsing? Browsing is the whole point of a place like this. # happy
-> END

=== jasper_thanks ===
Jasper: It'll be happier in your shop than gathering dust in mine. # happy
-> END

=== jasper_saturday ===
Jasper takes you rock-pooling and names every creature after a Victorian explorer. # happy
-> END

=== jasper_date ===
Jasper rows you out to watch the lighthouse come on, and tells you a story about every star he can name. # happy
-> END

// ---- More lines (2026-11-02: so nobody repeats themselves), and how they take a present ----
=== jasper_visit_key ===
Jasper: I found a lock that fits one of my orphan keys. Forty years in the drawer and it opened a biscuit tin. # surprised
Jasper: Empty. Of course. But still.
-> END
=== jasper_visit_storm ===
Jasper: Storm coming. The barometer in the shop's been sulking all morning. It's older than me and far more reliable. # thoughtful
-> END
=== jasper_saturday_charts ===
Jasper unrolls his oldest charts on the customs house floor. You find your great-aunt's handwriting in a margin, marking a fishing spot. # surprised
Jasper: She got around, your aunt. # happy
-> END
=== jasper_gift ===
Jasper: Something new for a man who sells old things. I'll keep it for a hundred years, and then it'll be an antique. # happy
-> END
