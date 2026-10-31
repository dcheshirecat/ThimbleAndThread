// Margaret "Midge" Lowe (she/her): runs the second-hand bookshop on Ropewalk, sixties, reads everything, sharp tongue. Friendship only. Chapter 2.
// Knot names follow Docs/ADDING_CONTENT.md, "Dialogue".

=== midge_intro ===
A woman with three pairs of glasses on chains peers at you over one of them. # thoughtful
Midge: You'll be the new one. I'm Midge. The bookshop on Ropewalk. Margaret, if you're a bank. # happy
~ met()
~ note(Runs the second-hand bookshop on Ropewalk.)
-> END

=== midge_visit ===
Midge: I'm on page four hundred of a murder mystery. It was the vicar. It's always the vicar. # happy
-> END

=== midge_visit_cat ===
Midge: The shop cat has decided the poetry section is his. Nobody buys poetry now. # sad
~ note(The bookshop cat sleeps in the poetry section.)
-> END

=== midge_visit_2_book ===
Midge: I found a stitching book in a box lot. 1931. I put it aside for you. # happy
-> END

=== midge_request_window ===
# fits: sampler, hoop_art
# best: sampler
Midge: My shop window has looked the same since 1989. # thoughtful
Midge: Something with words on it. Or near words. Something literate.
-> END

=== midge_request_bag ===
# fits: tote, scarf
# best: tote
Midge: I carry books home in my arms like a fool. # neutral
Midge: A bag. A strong one. I buy a lot of books.
-> END

=== midge_handover_good ===
Midge: Well. That's a first edition, that is. # happy
-> END

=== midge_handover_okay ===
Midge: Very good. I'll tell people. Selectively. # happy
-> END

=== midge_handover_poor ===
Midge: It's a rough draft. Everyone starts with a rough draft. # thoughtful
-> END

=== midge_suggest_yes ===
Midge: Yes, go on. # happy
-> END

=== midge_suggest_buy ===
Midge: I'll take the pattern. My mother taught me. I've forgotten. I'll remember. # happy
-> END

=== midge_suggest_no ===
Midge: No, thank you. Not my genre. # neutral
-> END

=== midge_scene_1 ===
Midge: Your great-aunt bought one book from me every week for twenty years. # thoughtful
Midge: Always mysteries. Always read the end first.
* [She read the ending first?]
    Midge: Said she liked to know who to root for. Monstrous woman. I adored her. # happy
    ~ friendship(10)
* [What was her favourite?]
    Midge: I'll find you a copy. It's got her notes in the margin. # happy
    ~ friendship(10)
-> END

=== midge_scene_2 ===
Midge: The landlord's putting the rent up. I might have to close. # sad
* [The town won't let that happen.]
    Midge: Towns let things happen all the time. But thank you. # thoughtful
    ~ friendship(10)
* [Put a sign in my window for you.]
    Midge: You'd do that? ... Fine. Yes. Thank you. # surprised
    ~ friendship(15)
-> END

=== midge_scene_3 ===
Midge: The shop's staying. Half the town came in and bought a book each. # surprised
Midge: I know it was you who told them. Don't deny it.
~ friendship(15)
-> END

=== midge_saturday ===
Midge closes the bookshop for an afternoon and you both read in the window seat while the rain comes down. # happy
Midge: Best kind of Saturday. Nobody wants anything from you. # happy
-> END
