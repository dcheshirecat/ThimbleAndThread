// Wenna Trelawney (she/her): seventy-four, runs the quilting bee at St. Brigid's. Brisk, kind, has opinions about seams.
// Friendship only. Chapter 3. Knot names follow Docs/ADDING_CONTENT.md, "Dialogue".

=== wenna_intro ===
A small woman in a quilted waistcoat inspects your shop window before she comes in. # thoughtful
Wenna: Wenna Trelawney. I run the quilting bee at the church. Thursdays, two till four, bring your own thimble. # happy
~ met()
~ note(Runs the quilting bee at St. Brigid's, Thursdays.)
Wenna: Your aunt came for thirty years. Never once on time. Always brought the best cake.
-> END

=== wenna_visit ===
Wenna: Quarter-inch seams. That's the whole secret of life, really. # thoughtful
-> END

=== wenna_visit_bee ===
Wenna: Twelve of us at the bee yesterday. Eleven sewing, one talking. Guess who. # happy
~ note(Says Dot talks through every quilting bee.)
-> END

=== wenna_visit_2_quilts ===
Wenna: I've made a quilt for every baby christened at St. Brigid's since 1979. Owen says I'm a parish institution. # happy
-> END

=== wenna_request_christening ===
# fits: quilt, plushie, plushie_sewn, sampler
# best: quilt
Wenna: There's a christening next month. Little Ivy, Mara's girl. # happy
Wenna: My hands aren't what they were. I'd like something soft for her, something that lasts longer than she stays small.
-> END

=== wenna_request_hall ===
# fits: wall_hanging, quilt, tablecloth, cushion
# best: wall_hanging
Wenna: The church hall is so bare. Grey walls, grey floor, grey vicar. # thoughtful
Wenna: Something to hang up. Something with a bit of colour in it.
-> END

=== wenna_handover_good ===
Wenna: Points match. Corners square. I'd have you at the bee any day. # happy
-> END

=== wenna_handover_okay ===
Wenna: Good honest work. It'll be loved. # happy
-> END

=== wenna_handover_poor ===
Wenna: Well. The first one never matches. Mine certainly didn't. # thoughtful
-> END

=== wenna_suggest_yes ===
Wenna: Oh, yes. Make me one of those. # happy
-> END

=== wenna_suggest_buy ===
Wenna: Could I buy the pattern? The bee needs new ideas, they're all making log cabins again. # happy
-> END

=== wenna_suggest_no ===
Wenna: Not for me, dear. Too fussy. # thoughtful
-> END

=== wenna_scene_1 ===
Wenna: Come to the bee on Thursday. No arguments. # happy
* [I'd love to.]
    Wenna: Bring cake. Your aunt set a standard. # happy
    ~ friendship(10)
* [I'm not much of a quilter.]
    Wenna: Nobody is, until they are. # thoughtful
    ~ friendship(10)
-> END

=== wenna_scene_2 ===
Wenna: Your aunt and I made a quilt together once. For the lifeboat crew, after the storm in '87. # sad
Wenna: Every patch was from someone's clothes. Shirts, aprons, a christening gown. It's still in the lifeboat station.
~ note(She and Aunt Win made a quilt for the lifeboat crew after the storm of 1987.)
* [I'd like to see it.]
    Wenna: I'll take you. Gwen keeps the key. # happy
    ~ friendship(10)
* [That's beautiful.]
    Wenna: It's patched in places now. That's how you know it's been loved. # thoughtful
    ~ friendship(10)
-> END

=== wenna_scene_3 ===
Wenna gives you her old thimble, worn smooth and silver.
Wenna: Seventy years on my finger. It wants a new finger. # happy
~ friendship(15)
-> END

=== wenna_saturday ===
You spend Saturday at Wenna's, sorting her fabric stash by colour. It takes all day. She has a lot of blue. # happy
Wenna: Never throw anything away. That's the other secret of life. # happy
-> END
