// Celeste Moreau (she/her): owns Moreau's, the boutique on the square (hair, clothes, accessories).
// Adult; romanceable. There from chapter 1. Blunt, stylish, secretly sentimental.
// Chat with her at the boutique (once a day); buying from her counts too. Friendship scenes at levels 1-3;
// at level 3 the player can ask her out (~ romance(1)); scenes 4 and 5 follow the romance if they did.

=== celeste_visit ===
Celeste: Ah. The new shopkeeper. Let me look at you. # thoughtful
~ note(Owns Moreau's, the boutique on the square.)
Celeste: Hm. Good bones. We can work with that.
-> END

=== celeste_visit_rain ===
Celeste: Rain's ruined my hair. No, don't look. Nobody look. # sad
-> END

=== celeste_visit_1_style ===
Celeste: You've got stitching on your sleeve again. # thoughtful
Celeste: It suits you. Don't tell anyone I said something suits someone.
-> END

=== celeste_visit_2_window ===
Celeste: I've redone my window. Your scarf is in the middle. # happy
Celeste: Purely because it's the best thing in it. Don't get ideas.
-> END

=== celeste_visit_4_coffee ===
Celeste: I brought you coffee. Oat milk, no sugar. I notice things. # happy
-> END

=== celeste_scene_1 ===
Celeste: Sit. I want to try something. # thoughtful
She tilts your chin to the light and frowns at you like a painting she hasn't finished.
Celeste: Everybody in this town dresses like they're apologising. You don't. Why not?
* [I wear what feels like me.]
    Celeste: Good answer. Rare answer. # happy
    ~ friendship(5)
* [Honestly? I just grab whatever's clean.]
    Celeste: ... I'm going to pretend you didn't say that. # surprised
    Celeste: But it works on you, which is infuriating.
    ~ friendship(5)
-> END

=== celeste_scene_2 ===
In the back room of Moreau's there's a sewing machine older than either of you, and a photograph of a woman in a magnificent hat.
Celeste: My grandmother. She made every hat in this town for fifty years. # thoughtful
~ note(Her grandmother made every hat in town for fifty years.)
Celeste: I can't sew to save my life. I just... arrange. It feels like cheating.
~ note(Can't sew at all. She arranges.)
* [Arranging is a craft too.]
    Celeste: You'd say that. You're kind. It's very annoying. # happy
    ~ friendship(10)
* [I could teach you to sew.]
    Celeste: You'd have to be patient. I'd have to be humble. One of us will fail. # happy
    ~ friendship(5)
-> END

=== celeste_scene_3 ===
Celeste: There's a gala at the old assembly rooms. Black tie. Terribly pretentious. # neutral
Celeste: I need someone on my arm who makes me look interesting.
* [Then I'd love to be on your arm.]
    Celeste: ... # surprised
    Celeste: Well. Good. I'll dress you. Obviously. # happy
    ~ romance(1)
    ~ friendship(10)
* [Ask Rowan, they'd love a gala.]
    Celeste: Rowan would arrive in a crocheted waistcoat and steal the room. # thoughtful
    Celeste: ... Actually, that's a good idea.
    ~ friendship(5)
-> END

=== celeste_scene_4 ===
* {romance} [The gala]
    She has dressed you in green. Of course she has. Everyone turns when you come in.
    Celeste: Don't look at them. Look at me. # happy
    ~ friendship(10)
* {not romance} [Help her dress the window]
    Celeste: You've got an eye. Don't let it go to waste in that wool shop. # happy
    ~ friendship(5)
-> END

=== celeste_scene_5 ===
* {romance} [Her grandmother's hat box]
    She opens a hat box. Inside is a little cloth flower, very badly stitched.
    Celeste: I made it. Tonight. With your needle. It took three hours and it's dreadful. # thoughtful
    Celeste: It's for you. It's the first thing I've ever made. I wanted it to be for you. # happy
    ~ flag(celeste_together)
    ~ friendship(15)
* {not romance} [Coffee at the window]
    Celeste: Best customer. Best friend. Don't tell anyone; I have a reputation. # happy
    ~ friendship(10)
-> END

// ---- At the boutique door (owner, 2026-10-31): a hello when you walk in, a goodbye when you leave
// (celeste_thanks* when you bought something). celeste_greet_2_... needs friendship level 2.

=== celeste_greet ===
Celeste: Come in. Touch anything you like, just not the silk. # thoughtful
-> END

=== celeste_greet_mirror ===
Celeste: The mirror's by the window. The light there is honest. Unlike my last boyfriend. # happy
-> END

=== celeste_greet_2_friend ===
Celeste: You again. Good. I've been saving something for you to try on. # happy
-> END

=== celeste_greet_4_close ===
Celeste: There you are. I'd started rearranging the rails out of boredom. # happy
-> END

=== celeste_bye ===
Celeste: Nothing today? The rails will survive. Barely. # thoughtful
-> END

=== celeste_bye_soon ===
Celeste: Off you go. Stand up straight on the way out, people are watching. # happy
-> END

=== celeste_thanks ===
Celeste: Excellent taste. Mine, mostly, but you chose it. # happy
-> END

=== celeste_thanks_wear ===
Celeste: Wear it out of the shop. Let the square see. # happy
-> END

=== celeste_thanks_2_proud ===
Celeste: You look wonderful. Don't make it weird. # happy
-> END
