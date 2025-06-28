def commit_callback(commit):
    if commit.author_email == b'demirkancevik@gmail.com':
        commit.author_email = b'demirkancevik48@gmail.com'
        commit.author_name = b'Demirkan Cevik'
    if commit.committer_email == b'demirkancevik@gmail.com':
        commit.committer_email = b'demirkancevik48@gmail.com'
        commit.committer_name = b'Demirkan Cevik'



