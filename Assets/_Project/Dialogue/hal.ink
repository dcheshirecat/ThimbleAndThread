// Hal Penrose (he/him): retired postman, sixty-eight. Knows every door in town and everyone behind it.
// Dry, grumbly, secretly soft; keeps racing pigeons. Friendship only. Arrives in chapter 1.

=== hal_intro ===
An older man in a postman's jacket with the badges taken off holds the door like he owns it. # thoughtful
Hal: Forty years I delivered to this door. Never once got to come inside. # thoughtful
Hal: Hal Penrose. Retired. Allegedly. # happy
~ met()
~ note(A retired postman: delivered to this door for forty years.)
Hal: Your great-aunt sent more parcels than the rest of the street put together. Patterns, mostly.
-> END

=== hal_visit ===
Hal: Just passing. I'm always just passing. It's a skill. # neutral
-> END

=== hal_visit_pigeons ===
Hal: Duchess came in third on Sunday. Third! She was robbed. # sad
~ note(Races pigeons. His best bird is called Duchess.)
-> END

=== hal_visit_rain ===
Hal: Forty years of weather and I still don't own a decent umbrella. Principle. # thoughtful
-> END

=== hal_visit_2_gossip ===
Hal: I'm not saying who, but someone on Harbour Row is getting a lot of parcels from a ukulele shop. # thoughtful
Hal: I'm not a gossip. I'm an observer.
-> END

=== hal_request_table ===
# fits: tablecloth, sampler
# best: tablecloth
Hal: The pigeon club's annual dinner is at mine this year. My table looks like a crime scene. # thoughtful
Hal: Something to make it look like a respectable man lives there.
-> END

=== hal_request_cold ===
# fits: scarf, sweater
# best: scarf
Hal: Up at the loft at dawn, the wind off the sea goes right through you. # neutral
Hal: My neck's had forty years of it. It'd like a holiday.
-> END

=== hal_request_duchess ===
# fits: hoop_art, sampler, plushie
# best: hoop_art
Hal: Duchess deserves a portrait. Don't laugh. # thoughtful
Hal: Something for the wall of the loft. A bird of her standing.
-> END

=== hal_handover_good ===
Hal: Well. That's... that's very good. Don't tell anyone I said so. # happy
-> END

=== hal_handover_okay ===
Hal: That'll do nicely. Better than nicely. Don't push it. # neutral
-> END

=== hal_handover_poor ===
Hal: Hm. It's got character. So do I. We'll get along. # thoughtful
-> END

=== hal_suggest_yes ===
Hal: Go on, then. Put me down for one. # happy
-> END

=== hal_suggest_buy ===
Hal: I'm not stitching anything. I'll have the finished one or nothing. # neutral
-> END

=== hal_suggest_no ===
Hal: Not really my sort of thing. I'm more of a... pigeon man. # thoughtful
-> END

=== hal_scene_1 ===
Hal pulls a bundle of envelopes from his coat, tied with string. # thoughtful
Hal: Letters nobody claimed. Forty years of them. I couldn't throw them out.
Hal: One's addressed to this shop. Postmarked before you were born.
* [Open it?]
    Hal: It's not mine to open. It's yours. # thoughtful
    Inside: a postcard of the harbour. "Win: the border was better your way. A." # surprised
    ~ note(Kept forty years of unclaimed letters.)
    ~ friendship(10)
* [Keep it for now.]
    Hal: Sensible. Some things want waiting for. # happy
    ~ friendship(5)
-> END

=== hal_scene_2 ===
Hal: Come and see the loft. If you're not busy. You're not busy. # happy
Thirty pigeons, a kettle, a radio, and a chair that's moulded to exactly one person.
Hal: My wife used to say I loved these birds more than her. # thoughtful
* [Did you?]
    Hal: No. But they're easier to talk to now she's gone. # sad
    ~ note(His wife has passed away; he talks to his pigeons.)
    ~ friendship(10)
* [Which one's Duchess?]
    Hal: The one looking at you like you owe her money. # happy
    ~ friendship(5)
-> END

=== hal_scene_3 ===
Hal: Duchess won. First place. The whole county. # surprised
Hal: I wore the scarf. For luck. Don't make a thing of it.
* [I'm making a thing of it.]
    Hal: I knew you would. Go on, then. Make a thing of it. # happy
    ~ friendship(10)
* [Congratulations, Hal.]
    Hal: Thank you. I mean it. You've made an old man's year. # happy
    ~ friendship(10)
-> END

=== hal_saturday ===
Hal lets you release Duchess from the clifftop and time her flight home. She beats you both. # happy
Hal: Never underestimate a bird with something to prove. # happy
-> END
