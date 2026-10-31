// Sol Reyes (they/them): a lifeguard on the main beach, twenty-six; surfs at dawn, sings badly, collects sea glass.
// Friendship only. Arrives in chapter 2.

=== sol_intro ===
Someone with salt-stiff hair and a wetsuit peeled to the waist drips cheerfully onto your doormat. # happy
Sol: Sorry, sorry. I'll stand on the mat. I'm Sol. Lifeguard. Beach. Big orange flag? # happy
~ met()
~ note(A lifeguard on the main beach; uses they/them.)
-> END

=== sol_visit ===
Sol: Waves were glass this morning. Pure glass. You should come out sometime. # happy
-> END

=== sol_visit_glass ===
Sol: Found a bit of blue sea glass. Blue's the rarest. I'm keeping it forever. # surprised
~ note(Collects sea glass; blue is their favourite.)
-> END

=== sol_visit_song ===
Sol: I've had the same song stuck in my head for three days. It's a jingle for a carpet shop. # sad
-> END

=== sol_visit_2_rescue ===
Sol: Pulled a dog out of a rip yesterday. Very ungrateful dog. Bit me. Still a good boy. # happy
-> END

=== sol_request_tower ===
# fits: tote, hoop_art
# best: tote
Sol: The lifeguard tower needs a bag for the first-aid stuff. Ours is held together with tape. # thoughtful
Sol: Something tough. Something that looks like it's been to sea and liked it.
-> END

=== sol_request_dawn ===
# fits: sweater, scarf
# best: sweater
Sol: Getting out of the water at six in the morning in March. You can't imagine. # sad
Sol: I need something to pull on that feels like a hug.
-> END

=== sol_request_glass ===
# fits: hoop_art, sampler, tablecloth
# best: sampler
Sol: My flat's walls are bare. All I've got is jars of sea glass. # thoughtful
Sol: Something with the sea in it. The colours of it.
-> END

=== sol_handover_good ===
Sol: Oh, that's stunning. I'm going to show literally everyone. Strangers. Gulls. # happy
-> END

=== sol_handover_okay ===
Sol: Nice one! That's going straight in my bag. # happy
-> END

=== sol_handover_poor ===
Sol: Hey, it's handmade. Handmade's the best kind. # happy
-> END

=== sol_suggest_yes ===
Sol: Yes! Love it. How long? No rush. Some rush. # happy
-> END

=== sol_suggest_buy ===
Sol: I've been meaning to learn. Can I take the pattern and try? # happy
-> END

=== sol_suggest_no ===
Sol: Hmm, not really me. I'm more... salty. # thoughtful
-> END

=== sol_scene_1 ===
Sol: Can I show you something? It's a bit daft. # thoughtful
They open a tin of sea glass, sorted by colour, every piece labelled with where and when they found it.
* [It's beautiful.]
    Sol: My gran started it. I kept going. # happy
    ~ note(Their gran started the sea glass collection.)
    ~ friendship(10)
* [We could stitch a chart from these colours.]
    Sol: Wait. Yes. Yes we could. # surprised
    ~ friendship(10)
-> END

=== sol_scene_2 ===
Sol: I nearly didn't take this job. I'm scared of deep water. # sad
Sol: Always have been. So I learned to be good at it.
* [That's the bravest thing I've heard.]
    Sol: Don't tell the other lifeguards. They think I'm fearless. # happy
    ~ friendship(10)
* [How did you learn?]
    Sol: One length at a time. Bit like your stitching, I bet. # thoughtful
    ~ friendship(5)
-> END

=== sol_scene_3 ===
Sol: I brought you something. # happy
A piece of blue sea glass, the rarest colour, wrapped in a scrap of paper.
Sol: For the shop window. For luck.
~ friendship(15)
-> END

=== sol_saturday ===
Sol takes you surfing. You stand up exactly once, for about a second. They cheer like you've won a medal. # happy
Sol: That's it! That's all surfing is! Standing up for a second! # happy
-> END
