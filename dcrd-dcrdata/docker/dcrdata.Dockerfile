FROM golang:1.10
ENV USER decred
RUN adduser --disabled-password --gecos ''  $USER

RUN go get -u -v github.com/golang/dep/cmd/dep
RUN mkdir -p $GOPATH/src/github.com/decred && \
    cd $GOPATH/src/github.com/decred && \
    curl -L --output v2.0.tar.gz https://github.com/decred/dcrdata/archive/v2.0.tar.gz && \
    tar -xvzf v2.0.tar.gz && \
    rm v2.0.tar.gz && \
    mv dcrdata-2.0 dcrdata && \
    cd dcrdata && \
    dep ensure && \
go install

USER $USER
WORKDIR $GOPATH/src/github.com/decred/dcrdata